using System;
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

        public override ValidationResult Validate()
        {
            var result = base.Validate();
            if (!result.Successful)
                return result;

            var path = ResolveWorkPath(WorkPath);
            if (File.Exists(path) || Directory.Exists(path))
                return ValidationResult.Success();

            return ValidationResult.Error($"Path not found: {path}");
        }
    }

    public static string ResolveWorkPath(string workPath)
    {
        return Path.GetFullPath(string.IsNullOrWhiteSpace(workPath)
            ? Directory.GetCurrentDirectory()
            : workPath);
    }

    public static bool TryGetFilesToProcess(string workPath, out string[] files, out string error)
    {
        var path = ResolveWorkPath(workPath);
        if (File.Exists(path))
        {
            files = [path];
            error = null;
            return true;
        }

        if (Directory.Exists(path))
        {
            files = Directory.GetFiles(path);
            error = null;
            return true;
        }

        files = [];
        error = $"Path not found: {path}";
        return false;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (!TryGetFilesToProcess(settings.WorkPath, out var files, out var error))
        {
            AnsiConsole.WriteLine(error);
            return 1;
        }

        foreach (var file in files)
        {
            try
            {
                ProcessFile(file, settings.FileNameDataFormat);
            }
            catch (Exception e)
            {
                AnsiConsole.WriteLine($"{file} {e.Message}");
            }
        }

        return 0;
    }

    private void ProcessFile(string filePath, string fileNameDataFormat)
    {
        if (!RenameService.IsMedia(filePath))
            return;
        var originFileName = Path.GetFileName(filePath);
        var changeRequired = renameService.ChangeRequired(originFileName, fileNameDataFormat);
        if (!changeRequired)
            return;
        var newName = renameService.GetNewName(filePath, fileNameDataFormat);

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
