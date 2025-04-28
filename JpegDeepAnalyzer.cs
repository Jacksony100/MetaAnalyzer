using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Meta
{
    public static class JpegDeepAnalyzer
    {
        public static string AnalyzeJpeg(string filePath)
        {
            var output = new StringBuilder();
            try
            {
                byte[] data = File.ReadAllBytes(filePath);

                if (!(data[0] == 0xFF && data[1] == 0xD8))
                {
                    output.AppendLine("Not a valid JPEG file.");
                    return output.ToString();
                }

                output.AppendLine("=== JPEG Segment Analysis ===");

                int pos = 2;
                while (pos < data.Length)
                {
                    if (data[pos] != 0xFF)
                    {
                        pos++;
                        continue;
                    }

                    byte marker = data[pos + 1];
                    int size = (data[pos + 2] << 8) + data[pos + 3];

                    string markerName = GetMarkerName(marker);
                    output.AppendLine($"Marker: 0x{marker:X2} ({markerName}), Size: {size} bytes at offset 0x{pos:X}");

                    pos += size + 2;
                }
            }
            catch (Exception ex)
            {
                output.AppendLine($"Error during JPEG analysis: {ex.Message}");
            }

            return output.ToString();
        }

        private static string GetMarkerName(byte marker)
        {
            switch (marker)
            {
                case 0xE0: return "APP0 (JFIF)";
                case 0xE1: return "APP1 (EXIF or XMP)";
                case 0xE2: return "APP2 (ICC)";
                case 0xED: return "APP13 (Photoshop)";
                case 0xFE: return "Comment";
                default:
                    if (marker >= 0xE0 && marker <= 0xEF)
                        return "APP" + (marker - 0xE0).ToString();
                    else
                        return "Other";
            }
        }
    }
}
