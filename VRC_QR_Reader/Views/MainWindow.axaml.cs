using System.Reactive.Disposables;
using Avalonia.Controls;
using ReactiveUI; 
using Avalonia.ReactiveUI;
using VRC_QR_Reader.ViewModels;

namespace VRC_QR_Reader.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(disposable =>
        {
            this.BindCommand(ViewModel,
                vm => vm.ProcessDirectory,
                v => v.ScanBtn
            ).DisposeWith(disposable);
            this.BindCommand(ViewModel,
                vm => vm.SaveToCsv,
                v => v.SaveBtn
            ).DisposeWith(disposable);
        });
    }
}