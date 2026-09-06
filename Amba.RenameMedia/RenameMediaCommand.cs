using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Amba.RenameMedia;

[Description("Gives date-time based name to images and videos")]
public class RenameMediaCommand : Command<RenameMediaCommand.Settings>
{
    private readonly RenameService renameService = new();

    public class Settings : CommandSettings
    {
        [CommandOption("-p|--path <PATH>")]
        [Description("Path to file or folder to process. Runs on current folder if empty.")]
        public string WorkPath { get; init; }

        [CommandOption("-d|--date-format|--df <FORMAT>")]
        [Description("Date time format. By default: yyyy-MM-dd HH-mm-ss")]
        [DefaultValue("yyyy-MM-dd HH-mm-ss")]
        public string FileNameDataFormat { get; init; } = @"yyyy-MM-dd HH-mm-ss";
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var imagesFolderPath = settings.WorkPath ?? Directory.GetCurrentDirectory();
        ProcessFolder(imagesFolderPath, settings.FileNameDataFormat);
        return 0;
    }

    private void ProcessFolder(string imagesFolderPath, string fileNameDataFormat)
    {
        foreach (var file in Directory.GetFiles(imagesFolderPath))
        {
            try
            {
                ProcessFile(file, fileNameDataFormat);
            }
            catch (Exception e)
            {
                AnsiConsole.WriteLine($"{file} {e.Message}");
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

    private void ProcessFile(string filePath, string fileNameDataFormat)
    {
        if (!IsMedia(filePath))
            return;
        var originFileName = Path.GetFileName(filePath);
        var changeRequired = renameService.ChangeRequired(originFileName, fileNameDataFormat);
        if (!changeRequired)
            return;
        var newName = renameService.GetNewName(originFileName, fileNameDataFormat);

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
        AnsiConsole.WriteLine($"{Path.GetFileName(filePath)}\t->\t{Path.GetFileName(newName)}");
    }
}
