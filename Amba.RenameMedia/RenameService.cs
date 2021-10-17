using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace Amba.RenameMedia
{
    //[assembly: InternalsVisibleTo("Amba.RenameMedia.Tests")]
    public class RenameService
    {
        private readonly List<Regex> knownFileFormats = new List<Regex>
        {
            // fixed format
            new Regex(@"^(\d\d\d\d)-(\d\d)-(\d\d) (\d\d)-(\d\d)-(\d\d)",  RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // android format
            new Regex(@"^[A-Z]{3}_(\d\d\d\d)(\d\d)(\d\d)_(\d\d)(\d\d)(\d\d)(\d\d\d).(mp4|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            
            // samsung format
            new Regex(@"^(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            
            // advocam format
            new Regex(@"^CarDV_(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        public string GetNewNameByKnownRegex(string fileName, string fileNameDataFormat)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            //try extract date from android file name
            foreach (var formatRegex in knownFileFormats)
            {
                var fixedFormatMatch = formatRegex.Match(fileName);
                if (fixedFormatMatch.Success)
                {
                    var year = Int32.Parse(fixedFormatMatch.Groups[1].Value);
                    var month = Int32.Parse(fixedFormatMatch.Groups[2].Value);
                    var day = Int32.Parse(fixedFormatMatch.Groups[3].Value);

                    var hour = Int32.Parse(fixedFormatMatch.Groups[4].Value);
                    var min = Int32.Parse(fixedFormatMatch.Groups[5].Value);
                    var sec = Int32.Parse(fixedFormatMatch.Groups[6].Value);
                    
                    return (new DateTime(year, month, day, hour, min, sec, DateTimeKind.Utc)).ToString(fileNameDataFormat) + extension;
                }
            }

            return string.Empty;
        }
        
        public string GetNewName(string fileName, string fileNameDataFormat)
        {
            var newName = GetNewNameByKnownRegex(fileName, fileNameDataFormat);
            
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            //try extract date from EXIF
            if (string.IsNullOrWhiteSpace(newName) && extension == ".jpg")
            {
                try
                {
                    using var image = SixLabors.ImageSharp.Image.Load(fileName);
                    var creationDate = GetExifCreationDate(image);
                    if (creationDate != null)
                    {
                        newName = creationDate.Value.ToString(fileNameDataFormat) + extension;
                    }
                }
                catch (Exception e)
                {
                    // do nothing if image reader cannot read the file
                }
            }

            //get datetime from file info
            if (string.IsNullOrWhiteSpace(newName))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(fileName);
                if (lastWriteTime != DateTime.MinValue)
                {
                    newName = lastWriteTime.ToString(fileNameDataFormat) + extension;
                }
            }
            return newName;
        }

        private DateTime? GetExifCreationDate(Image image)
        {
            if (image.Metadata?.ExifProfile?.Values == null)
                return null;
            var date = image.Metadata.ExifProfile.Values.FirstOrDefault(x => x.Tag == ExifTag.DateTimeOriginal);
            if (date == null)
            {
                date = image.Metadata.ExifProfile.Values.FirstOrDefault(x => x.Tag == ExifTag.DateTimeDigitized);
            }

            if (date == null)
            {
                date = image.Metadata.ExifProfile.Values.FirstOrDefault(x => x.Tag == ExifTag.DateTime);
            }

            if (date != null)
            {
                DateTime.TryParseExact(date.GetValue().ToString(), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var result);
                return result;
            }

            return null;
        }

        public bool ChangeRequired(string originFileName, string fileNameDataFormat)
        {
            var fileName = Path.GetFileNameWithoutExtension(originFileName);
            var dateParsed = DateTime.TryParseExact(fileName, fileNameDataFormat, CultureInfo.InvariantCulture,  DateTimeStyles.None, out DateTime result);
            return !dateParsed;
        }
    }
} 