using System;
using System.Collections.Generic;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using System.Diagnostics.CodeAnalysis;

namespace Amba.RenameMedia;

[Command("rename-media", Description = "Gives date-time based name to images and videos")]
[HelpOption("--help|-h")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class RenameMediaCommand(RenameService renameService)
{
    [Option("-p|--path", CommandOptionType.SingleValue, Description = "Path to file or folder to process. Runs on current folder if empty.")]
    public string WorkPath { get; set; }

    [Option("-df|--date-format", CommandOptionType.SingleValue, Description = "Date time format. By default: yyyy-MM-dd HH-mm-ss")]
    public string FileNameDataFormat { get; set; } = @"yyyy-MM-dd HH-mm-ss";


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
                Console.WriteLine($"{file} {e.Message}");
            }
        }
    }
    
    private bool IsMedia(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var imageExtensions = new HashSet<string>
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".svg"
            // raw formats
            , ".cr2", ".nef", ".dng", ".arw", ".orf", ".rw2", ".raf", ".pef", ".srw", ".x3f", ".mrw", ".nrw", ".kdc"
            // video formats
            , ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".vob", ".ogv", ".ogg", ".gifv", ".m4v", ".3gp", ".3g2"
        };
        return imageExtensions.Contains(extension);
    }


    private void ProcessFile(string filePath)
    {
        if (!IsMedia(filePath))
            return;
        var originFileName = Path.GetFileName(filePath);
        var changeRequired = renameService.ChangeRequired(originFileName, FileNameDataFormat);
        if (!changeRequired)
            return;
        var newName = renameService.GetNewName(originFileName, FileNameDataFormat);
            
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
