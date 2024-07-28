using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using ReactiveUI.Fody.Helpers;
using ZXing;
using SixLabors.ImageSharp.PixelFormats;
using VRC_QR_Reader.Models;
using System.Media;
using System.Reactive;
using ReactiveUI;
using Image = SixLabors.ImageSharp.Image;


namespace VRC_QR_Reader.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    [Reactive]public string Title { get; set; }

    [Reactive]public ObservableCollection<UrlItem> UrlObjects { get; set; }

    [Reactive]private FileSystemWatcher FsWatcher { get; set; } 
    
    [Reactive]public string QrCodesPath { get; set; }
    
    [Reactive]public bool AutoOpen { get; set; }
    [Reactive]public bool PlaySound { get; set; }
    [Reactive]public bool VrcXCompat { get; set; }
    
    public ReactiveCommand<Unit,Unit> ProcessDirectory { get; }
    public ReactiveCommand<Unit,Unit> SaveToCsv { get; }
    
    //TODO: Figure out double taps
    //public ReactiveCommand<Unit,Unit> DblClick { get; }
    


    public MainWindowViewModel()
    {
        ProcessDirectory = ReactiveCommand.Create(DoProcessDirectory); 
        SaveToCsv = ReactiveCommand.Create(DoSave);
        VrcXCompat = true;
        //TODO: Double Click event
        //DblClick = ReactiveCommand.Create(DoDoubleClick);
        
        Title = "VRChat QR Code Utilities";
        AutoOpen = false;
        UrlObjects = new ObservableCollection<UrlItem>() { };
        QrCodesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "VRChat");
        FsWatcher = new FileSystemWatcher()
        {
            Path = QrCodesPath, 
            Filter = "*.png",
            IncludeSubdirectories = true
        };
        
        FsWatcher.Created += FsWatcherOnCreated;
        FsWatcher.Renamed += FsWatcherOnRenamed;
        
        FsWatcher.EnableRaisingEvents = true;
    }
    private void FsWatcherOnRenamed(object sender, FileSystemEventArgs e)
    {
        if (VrcXCompat && !e.FullPath.Contains("wrld")) return; // For VRCX Compatibility, wait until file has the world tag in name
        ProcessFile(e.FullPath);
    }
    private void FsWatcherOnCreated(object sender, FileSystemEventArgs e)
    {
        if (VrcXCompat && !e.FullPath.Contains("wrld")) return; // For VRCX Compatibility, wait until file has the world tag in name
        ProcessFile(e.FullPath);
    }

    private void ProcessFile(string path, bool processDir=false)
    {
        while (IsFileLocked(path))
        {
            Thread.Sleep(100);
        }

        var result = DecodeQrCode(path);

        if (result == "NOCODE") return;
        
        if (PlaySound && !processDir) SystemSounds.Asterisk.Play();
        
        UrlObjects.Add(new UrlItem(result,Path.GetFileName(path)));
        if (AutoOpen && !processDir) System.Diagnostics.Process.Start("explorer",result);
    }

    private string DecodeQrCode(string imagePath)
    {
        string output = "NOCODE";
        var barcodeReader = new BarcodeReaderGeneric();
        var result = barcodeReader.Decode(Image.Load<Rgba32>(imagePath));
        if (result != null)
        {
            output = result.Text;
        }
        return output;
    }
    
    private bool IsFileLocked(string path)
    {
        FileStream stream = null;

        try
        {
            FileInfo file = new FileInfo(path);
            stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch (Exception e)
        {
            return true;
        }
        finally
        {
            if (stream != null) { stream.Close(); }
        }

        return false;
    }

    private void DoProcessDirectory()
    {
        string[] vrcScreenshots = Directory.GetFiles(QrCodesPath, "*.png", SearchOption.AllDirectories);
         foreach (var file in vrcScreenshots)
        {
            ProcessFile(file, true);
        }
    }

    private void DoSave()
    {
        //TODO: use file dialogs 

        string csvGlob = "'URI','Name','Description','FileName'";
        string htmlGlob = "<HTML><HEAD><TITLE>QR Codes</TITLE></HEAD><BODY>";
        foreach (var obj in UrlObjects)
        {
            csvGlob += obj.ToString() + "<BR/>\n";
            htmlGlob += obj.ToHtml();
        }

        htmlGlob += "</BODY></HTML>";
        
        File.WriteAllText("QRCodes.csv",csvGlob);
        File.WriteAllText("QRCodes.html",htmlGlob);

    }

}