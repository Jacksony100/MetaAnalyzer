using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meta
{
        public static class MainWindowStaticHelper
        {
            public static string ExtractDocxMetadataStatic(string filePath)
            {
                using (var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(filePath, false))
                {
                    var coreProps = doc.PackageProperties;
                    string output = "=== DOCX Metadata ===\n";
                    output += $"Title: {coreProps.Title}\n";
                    output += $"Author: {coreProps.Creator}\n";
                    output += $"Created: {coreProps.Created}\n";
                    output += $"Modified: {coreProps.Modified}\n";
                    output += $"Keywords: {coreProps.Keywords}\n";
                    output += $"Description: {coreProps.Description}\n";
                    return output;
                }
            }
        }
}
