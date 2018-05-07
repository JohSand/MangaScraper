using System;
using WixSharp;

namespace WixSharpInstaller {
  class Program {
    static int Main() {
      try {
        //DON'T FORGET to add NuGet package "WixSharp".

        var project = new Project("MangaScraper",
          new Dir(@"%ProgramFiles%\MangaScraper\",
            new Files(@"MangaScraper.UI\bin\Release\*.*")));

        project.GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25889b");
        //project.SourceBaseDir = "<input dir path>";
        //project.OutDir = "<output dir path>";
        project.SourceBaseDir = System.IO.Path.Combine(Environment.CurrentDirectory, "..\\");
        project.LicenceFile = "lic.rtf";
        project.BuildMsi();
      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
        return 1;
      }
      return 0;
    }
  }
}