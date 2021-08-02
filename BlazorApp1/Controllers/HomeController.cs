using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Syncfusion.EJ2.FileManager.Base;


namespace BlazorApp1.Controllers
{
  public class FileResponse
  {
    public DirectoryContent CWD { get; set; }
    public IEnumerable<DirectoryContent> Files { get; set; }
    public ErrorDetails Error { get; set; }
    public FileDetails Details { get; set; }
  }

  public class DirectoryContent
  {
    public FileManagerDirectoryContent[] Data { get; set; }
    public bool ShowHiddenItems { get; set; }
    public string SearchString { get; set; }
    public bool CaseSensitive { get; set; }
    public IList<IFormFile> UploadFiles { get; set; }
    public string[] RenameFiles { get; set; }
    public string TargetPath { get; set; }
    public string ParentId { get; set; }
    public string FilterId { get; set; }
    public string FilterPath { get; set; }
    public string Id { get; set; }
    public string Type { get; set; }
    public bool IsFile { get; set; }
    public bool HasChild { get; set; }
    public string URL { get; set; }
    public string UrlValue { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
    public string PreviousName { get; set; }
    public long Size { get; set; }
    public string Name { get; set; }
    public string[] Names { get; set; }
    public string NewName { get; set; }
    public string Action { get; set; }
    public string Path { get; set; }
    public FileManagerDirectoryContent TargetData { get; set; }
    public AccessPermission Permission { get; set; }
  }

  [Route("api/[controller]")]
    public class HomeController : Controller
    {
    public PhysicalFileProvider operation;
    public string basePath;
    string root = "wwwroot\\Files";
    public static string custom_id { get; set; }
    public HomeController(IHostingEnvironment hostingEnvironment)
    {
      this.basePath = hostingEnvironment.ContentRootPath;
      this.operation = new PhysicalFileProvider();
      this.operation.RootFolder(this.basePath + "\\" + this.root);
    }
    public object getFiles(FileManagerDirectoryContent args)
    {
      FileResponse readResponse = new FileResponse();
      try
      {
        var value = this.operation.GetFiles(args.Path, args.ShowHiddenItems);
        DirectoryContent cwd = new DirectoryContent();
        readResponse.CWD = JsonConvert.DeserializeObject<DirectoryContent>(JsonConvert.SerializeObject(value.CWD));
        readResponse.CWD.URL = "https://github.com/SyncfusionExamples/ej2-aspcore-file-provider";
        readResponse.Files = JsonConvert.DeserializeObject<IEnumerable<DirectoryContent>>(JsonConvert.SerializeObject(value.Files));
        //Add the additional parameter for each files in filemanager component.
        foreach (DirectoryContent file in readResponse.Files)
        {
          //Add the URL as additional parameter.
          file.URL = "https://www.google.com/";
          // Add the URL value as addittional parameter.
          file.UrlValue = "Google";
        }
        readResponse.Details = value.Details;
        readResponse.Error = value.Error;
        return readResponse;
      }
      catch
      {
        ErrorDetails er = new ErrorDetails();

      }
      return this.operation.ToCamelCase(this.operation.GetFiles(args.Path, args.ShowHiddenItems));
    }

    [Route("FileOperations")]
    public object FileOperations([FromBody] FileManagerDirectoryContent args)
    {
         
      if (args.Action == "delete" || args.Action == "rename")
      {
        if ((args.TargetPath == null) && (args.Path == ""))
        {
          FileManagerResponse response = new FileManagerResponse();
          response.Error = new ErrorDetails { Code = "401", Message = "Restricted to modify the root folder." };
          return this.operation.ToCamelCase(response);
        }
      }
      switch (args.Action)
      {
        case "read":
          // reads the file(s) or folder(s) from the given path.
          //return this.operation.ToCamelCase(this.operation.GetFiles(args.Path, args.ShowHiddenItems));
          return this.getFiles(args);
        case "delete":
          // deletes the selected file(s) or folder(s) from the given path.
          return this.operation.ToCamelCase(this.operation.Delete(args.Path, args.Names));
        case "copy":
          // copies the selected file(s) or folder(s) from a path and then pastes them into a given target path.
          return this.operation.ToCamelCase(this.operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
        case "move":
          // cuts the selected file(s) or folder(s) from a path and then pastes them into a given target path.
          return this.operation.ToCamelCase(this.operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
        case "details":
          // gets the details of the selected file(s) or folder(s).
          return this.operation.ToCamelCase(this.operation.Details(args.Path, args.Names, args.Data));
        case "create":
          // creates a new folder in a given path.
          return this.operation.ToCamelCase(this.operation.Create(args.Path, args.Name));
        case "search":
          // gets the list of file(s) or folder(s) from a given path based on the searched key string.
          return this.operation.ToCamelCase(this.operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive));
        case "rename":
          // renames a file or folder.
          return this.operation.ToCamelCase(this.operation.Rename(args.Path, args.Name, args.NewName));
      }
      return null;
    }

    // uploads the file(s) into a specified path
    [Route("Upload")]
    public IActionResult Upload(string path, IList<IFormFile> uploadFiles, string action, string CustomData)
    {
    
      FileManagerResponse uploadResponse;
      uploadResponse = operation.Upload(path, uploadFiles, action, null);
      if (uploadResponse.Error != null)
      {
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
        Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
      }
      return Content("");
    }

    // downloads the selected file(s) and folder(s)
    [Route("Download")]
    public IActionResult Download(string downloadInput)
    {
    
      FileManagerDirectoryContent args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(downloadInput);
      return operation.Download(args.Path, args.Names, args.Data);
    }

    // gets the image(s) from the given path
    [Route("GetImage")]
    public IActionResult GetImage(FileManagerDirectoryContent args)
    {
     
      return this.operation.GetImage(args.Path, args.Id, false, null, null);
    }
    public IActionResult Index()
        {
            return View();
        }
    }
}
