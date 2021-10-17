using System;
using Xunit;

namespace Amba.RenameMedia.Test
{
    public class RenameServiceTest
    {
        [Theory()]
        [InlineData("2020-11-18 23-54-24.mp4", @"yyyy-MM-dd HH-mm-ss", false)]
        [InlineData("20201118_235424.mp4", @"yyyy-MM-dd HH-mm-ss", true)]
        [InlineData("x.mp4", @"yyyy-MM-dd HH-mm-ss", true)]
        [InlineData("2020-11-18 23-54.mp4", @"yyyy-MM-dd HH-mm-ss", true)]
        public void ChangeRequiredTest(string fileName, string format, bool expectedChangeRequired)
        {
            var renameService = new RenameService();
            var actualChangeRequired = renameService.ChangeRequired(fileName, format);
            Assert.Equal(expectedChangeRequired, actualChangeRequired);
        }

        private const string FileDateFomat = @"yyyy-MM-dd HH-mm-ss";
        
        [Theory()]
        [InlineData("20201118_235424.mp4", @"2020-11-18 23-54-24.mp4")]
        [InlineData("x.mp4", @"")]
        [InlineData("PXL_20211017_161031207.jpg", @"2021-10-17 16-10-31.jpg")]
        [InlineData("CarDV_20211005_210716A.MP4", @"2021-10-05 21-07-16.mp4")]
        public void GetNewNameByKnownRegexTest(string fileName, string expectedNewName)
        {
            var renameService = new RenameService();
            var actualNewName = renameService.GetNewNameByKnownRegex(fileName, FileDateFomat);
            Assert.Equal(expectedNewName, actualNewName);
        }
    }
}