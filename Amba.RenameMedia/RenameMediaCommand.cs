using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using System.Diagnostics.CodeAnalysis;

namespace Amba.RenameMedia;

[Command("rename-media", Description = "Gives date-time based name to images and videos")]
[HelpOption("--help|-h")]
public class RenameMediaCommand 
{
    private readonly RenameService _renameService;

    [Option("-p|--path", CommandOptionType.SingleValue, Description = "Path to file or folder to process. Runs on current folder if empty.")]
    public string WorkPath { get; set; }

    [Option("-df|--date-format", CommandOptionType.SingleValue, Description = "Date time format. By default: yyyy-MM-dd HH-mm-ss")]
    public string FileNameDataFormat { get; set; } = @"yyyy-MM-dd HH-mm-ss";

    public RenameMediaCommand(RenameService renameService)
    {
        _renameService = renameService;
    }
        
    [RequiresUnreferencedCode("OnExecute used by CLI Interface")]
    public int OnExecute()
    {
        var imagesFolderPath = WorkPath ??  Directory.GetCurrentDirectory();
        ProcessFolder(imagesFolderPath);
        return 0;
    }

    private void ProcessFolder(string imagesFolderPath)
    {
        foreach (var file in Directory.GetFiles(imagesFolderPath))
        {
            try
            {
                ProcessFile(file);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    private void ProcessFile(string filePath)
    {
        var originFileName = Path.GetFileName(filePath);
        var changeRequired = _renameService.ChangeRequired(originFileName, FileNameDataFormat);
        if (!changeRequired)
            return;
        var newName = _renameService.GetNewName(originFileName, FileNameDataFormat);
            
        if (string.IsNullOrWhiteSpace(newName))
        {
            // can't find new name
            return;
        }

        if (originFileName == newName)
        {
            // no change needed
            return;
        }
            
        var folder = Path.GetDirectoryName(filePath);
        var newPath = Path.Combine(folder, newName);
        if (File.Exists(newPath))
        {
            newPath = Path.Combine(folder,
                Path.GetFileNameWithoutExtension(newName) + "_" + Path.GetFileName(filePath));
        }
        File.Move(filePath, newPath);
        Console.WriteLine($"{Path.GetFileName(filePath)}\t->\t{Path.GetFileName(newName)}");
    }
}
