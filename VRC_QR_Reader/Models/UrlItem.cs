using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace VRC_QR_Reader.Models;

public class UrlItem : ReactiveObject
{
    [Reactive] public string Uri { get; set; }
    [Reactive] public string Description { get; set; } 
    [Reactive] public string Name { get; set; } 
    [Reactive] public string FileName { get; set; } 

    public UrlItem(string uri, string filename)
    {
        Uri = uri;
        try
        {
            var page = CallUrl(uri).Result;
            Name = GetTitle(page);
            Description = GetMetaDescription(page);
        } catch
        {
            Name = "Bad URL";
            Description = "Bad URL";
        }
        FileName = filename;
    }
    
    private static async Task<HtmlDocument> CallUrl(string fullUrl)
    {
        var web = new HtmlWeb();
        web.OverrideEncoding = Encoding.UTF8;
        var document = web.Load(fullUrl);
        return document;
    }
    private static string GetTitle(HtmlDocument document)
    {
        var titleNode = document.DocumentNode.SelectSingleNode("//title");
        var retVal = titleNode?.InnerText.Trim();
        
        return retVal == null ? "NO TITLE": retVal;
    }
    private static string GetMetaDescription(HtmlDocument document)
    {
        var metaDescriptionNode = document.DocumentNode.SelectSingleNode("//meta[@name='description']");
        return metaDescriptionNode == null ? "NO DESC" : metaDescriptionNode.GetAttributeValue("content", "").Trim();
    }
    
    public override string ToString()
    {
        return "'"+Uri+"','"+Name+"','"+Description+"','"+FileName+"'";
    }

    public string ToHtml()
    {
        return "<A HREF='" + Uri + "'> " + Name + "</A>\n";
    }
}