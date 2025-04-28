using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meta
{
    public static class ExifAnalyzer
    {
        public static string ExtractExifMetadata(string filePath)
        {
            var output = new StringBuilder();
            try
            {
                output.AppendLine("=== EXIF Metadata ===");

                var directories = ImageMetadataReader.ReadMetadata(filePath);
                foreach (var directory in directories)
                {
                    foreach (var tag in directory.Tags)
                    {
                        output.AppendLine($"{directory.Name} - {tag.Name}: {tag.Description}");
                        if (!string.IsNullOrEmpty(tag.Description) && tag.Description.Length > 200)
                        {
                            output.AppendLine($"⚠ Warning: Tag {tag.Name} unusually long ({tag.Description.Length} characters)");
                        }
                    }
                }

                output.AppendLine();
            }
            catch (System.Exception ex)
            {
                output.AppendLine($"Error reading EXIF: {ex.Message}");
            }

            return output.ToString();
        }
    }
}
