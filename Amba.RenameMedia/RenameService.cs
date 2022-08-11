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
            new Regex(@"^CarDV_(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            
            // iphone ieic format
            new Regex(@"(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})\d+_iOS.(mp4|jpg|heic|mov)", RegexOptions.Compiled | RegexOptions.IgnoreCase)
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
            // try generate new name different ways
            
            var newName = GetNewNameByKnownRegex(fileName, fileNameDataFormat);
            if (!string.IsNullOrEmpty(newName))
                return newName;
            
            //try extract date from EXIF
            if (IsJpeg(fileName))
            {
                newName = GetNewNameByExifDate(fileName, fileNameDataFormat);
                if (!string.IsNullOrEmpty(newName))
                    return newName;
            }

            //get datetime from file info
            DateTime lastWriteTime = File.GetLastWriteTime(fileName);
            if (lastWriteTime != DateTime.MinValue)
            {
                newName = lastWriteTime.ToString(fileNameDataFormat) + Path.GetExtension(fileName);
                if (!string.IsNullOrEmpty(newName))
                    return newName;
            }
            return string.Empty;
        }


        private readonly HashSet<string> _jpegExtensions = new() { ".jpg", ".jpeg" }; 
        private bool IsJpeg(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _jpegExtensions.Contains(extension);
        }
        
        private string GetNewNameByExifDate(string fileName, string fileNameDataFormat)
        {
            string newName = string.Empty;
            try
            {
                using var image = Image.Load(fileName);
                var creationDate = GetExifCreationDate(image);
                if (creationDate != null)
                {
                    newName = creationDate.Value.ToString(fileNameDataFormat) + Path.GetExtension(fileName);
                }
            }
            catch
            {
                // do nothing if image reader cannot read the file
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
            fileName = fileName.Length > fileNameDataFormat.Length
                ? fileName.Substring(0, fileNameDataFormat.Length)
                : fileName;
            var dateParsed = DateTime.TryParseExact(fileName, fileNameDataFormat, CultureInfo.InvariantCulture,  DateTimeStyles.None, out DateTime result);
            return !dateParsed;
        }
    }
} 