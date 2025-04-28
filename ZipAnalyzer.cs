using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Meta
{
    using ICSharpCode.SharpZipLib.Zip;
    using System;
    using System.IO;
    using System.Text;
    using System.IO.Compression;

    namespace MetadataAnalyzer
    {
        public static class ZipAnalyzer
        {
            public static string AnalyzeZip(string filePath)
            {
                StringBuilder output = new StringBuilder();
                string tempFolder = Path.Combine(Path.GetTempPath(), "MetadataAnalyzer_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempFolder);

                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(filePath, tempFolder);

                    output.AppendLine($"=== ZIP Archive Metadata ===");
                    output.AppendLine($"Extracted to: {tempFolder}");
                    output.AppendLine();

                    AnalyzeDirectory(tempFolder, output);

                    output.AppendLine();
                    output.AppendLine("=== End of ZIP Content ===");
                }
                catch (Exception ex)
                {
                    output.AppendLine($"Error analyzing ZIP: {ex.Message}");
                }

                return output.ToString();
            }

            private static void AnalyzeDirectory(string folderPath, StringBuilder output)
            {
                foreach (string file in Directory.GetFiles(folderPath))
                {
                    output.AppendLine($"Found file: {Path.GetFileName(file)}");

                    string realType = DetectFileType(file);

                    output.AppendLine($"Detected type: {realType}");

                    switch (realType)
                    {
                        case "jpg":
                        case "png":
                            output.AppendLine(ExifAnalyzer.ExtractExifMetadata(file));
                            output.AppendLine(StegoAnalyzer.CheckStego(file));
                            break;

                        case "pdf":
                            output.AppendLine(PdfAnalyzer.ExtractPdfMetadata(file));
                            break;

                        case "docx":
                            output.AppendLine(MainWindowStaticHelper.ExtractDocxMetadataStatic(file));
                            break;

                        case "zip":
                            output.AppendLine("Nested ZIP detected (not unpacking recursively).");
                            break;

                        default:
                            output.AppendLine("Unknown or unsupported file type.");
                            break;
                    }

                    output.AppendLine();
                }

                foreach (string dir in Directory.GetDirectories(folderPath))
                {
                    AnalyzeDirectory(dir, output);
                }
            }

            private static string DetectFileType(string filePath)
            {
                byte[] buffer = new byte[8];
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(buffer, 0, buffer.Length);
                }

                if (buffer[0] == 0xFF && buffer[1] == 0xD8)
                    return "jpg";
                if (buffer[0] == 0x89 && buffer[1] == 0x50)
                    return "png";
                if (buffer[0] == 0x25 && buffer[1] == 0x50)
                    return "pdf";
                if (buffer[0] == 0x50 && buffer[1] == 0x4B)
                    return "zip";

                return "unknown";
            }

            public static bool ContainsZipSignature(byte[] data)
            {
                byte[] zipSignature = { 0x50, 0x4B, 0x03, 0x04 };
                for (int i = 0; i < data.Length - zipSignature.Length; i++)
                {
                    bool found = true;
                    for (int j = 0; j < zipSignature.Length; j++)
                    {
                        if (data[i + j] != zipSignature[j])
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                        return true;
                }
                return false;
            }
        }
    }

}
