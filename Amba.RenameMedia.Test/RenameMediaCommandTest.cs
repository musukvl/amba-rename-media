using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Amba.RenameMedia.Test;

public class RenameMediaCommandTest
{
    [Fact]
    public void TryGetFilesToProcess_ReturnsTheFile_WhenPathIsAFile()
    {
        var tempDirectory = Directory.CreateTempSubdirectory();
        try
        {
            var filePath = Path.Combine(tempDirectory.FullName, "shot.heic");
            File.WriteAllBytes(filePath, [1, 2, 3]);

            var found = RenameMediaCommand.TryGetFilesToProcess(filePath, out var files, out var error);

            Assert.True(found);
            Assert.Null(error);
            Assert.Equal([Path.GetFullPath(filePath)], files);
        }
        finally
        {
            tempDirectory.Delete(true);
        }
    }

    [Fact]
    public void TryGetFilesToProcess_ReturnsDirectoryFiles_WhenPathIsAFolder()
    {
        var tempDirectory = Directory.CreateTempSubdirectory();
        try
        {
            File.WriteAllBytes(Path.Combine(tempDirectory.FullName, "a.jpg"), [1]);
            File.WriteAllBytes(Path.Combine(tempDirectory.FullName, "b.heic"), [2]);
            Directory.CreateDirectory(Path.Combine(tempDirectory.FullName, "nested"));
            File.WriteAllBytes(Path.Combine(tempDirectory.FullName, "nested", "c.jpg"), [3]);

            var found = RenameMediaCommand.TryGetFilesToProcess(tempDirectory.FullName, out var files, out var error);

            Assert.True(found);
            Assert.Null(error);
            var names = files.Select(Path.GetFileName).OrderBy(name => name).ToArray();
            Assert.Equal(["a.jpg", "b.heic"], names);
        }
        finally
        {
            tempDirectory.Delete(true);
        }
    }

    [Fact]
    public void TryGetFilesToProcess_Fails_WhenPathDoesNotExist()
    {
        var missing = Path.Combine(Path.GetTempPath(), "amba-rename-media-missing-" + Guid.NewGuid());

        var found = RenameMediaCommand.TryGetFilesToProcess(missing, out var files, out var error);

        Assert.False(found);
        Assert.Empty(files);
        Assert.Contains("Path not found:", error);
        Assert.Contains(Path.GetFullPath(missing), error);
    }

    [Fact]
    public void SettingsValidate_Succeeds_ForExistingFile()
    {
        var tempDirectory = Directory.CreateTempSubdirectory();
        try
        {
            var filePath = Path.Combine(tempDirectory.FullName, "shot.jpg");
            File.WriteAllBytes(filePath, [1]);
            var settings = new RenameMediaCommand.Settings { WorkPath = filePath };

            var result = settings.Validate();

            Assert.True(result.Successful);
        }
        finally
        {
            tempDirectory.Delete(true);
        }
    }

    [Fact]
    public void AppVersion_MatchesProjectVersion()
    {
        Assert.Matches(@"^2\.0\.0(\+.+)?$", AppVersion.Current);
    }

    [Fact]
    public void SettingsValidate_Fails_WhenPathDoesNotExist()
    {
        var missing = Path.Combine(Path.GetTempPath(), "amba-rename-media-missing-" + Guid.NewGuid());
        var settings = new RenameMediaCommand.Settings { WorkPath = missing };

        var result = settings.Validate();

        Assert.False(result.Successful);
        Assert.Contains("Path not found:", result.Message);
    }
}
