using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Meta
{
    public static class PngDeepAnalyzer
    {
        public static string AnalyzePng(string filePath)
        {
            var output = new StringBuilder();
            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                byte[] pngSignature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
                for (int i = 0; i < pngSignature.Length; i++)
                {
                    if (data[i] != pngSignature[i])
                    {
                        output.AppendLine("Not a valid PNG file.");
                        return output.ToString();
                    }
                }

                output.AppendLine("=== PNG Chunk Analysis ===");

                int pos = 8;
                while (pos + 8 < data.Length)
                {
                    int length = ReadInt32(data, pos);
                    string chunkType = Encoding.ASCII.GetString(data, pos + 4, 4);

                    output.AppendLine($"Chunk: {chunkType}, Length: {length} bytes at offset 0x{pos:X}");

                    pos += length + 12;

                    if (chunkType == "IEND")
                        break;
                }

                if (pos < data.Length - 1)
                {
                    output.AppendLine("⚠️ Data detected after IEND! Possible hidden data.");
                }
            }
            catch (Exception ex)
            {
                output.AppendLine($"Error during PNG analysis: {ex.Message}");
            }

            return output.ToString();
        }

        private static int ReadInt32(byte[] data, int offset)
        {
            return (data[offset] << 24) | (data[offset + 1] << 16) |
                   (data[offset + 2] << 8) | data[offset + 3];
        }
    }
}
