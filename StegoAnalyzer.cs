using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meta
{
    public static class StegoAnalyzer
    {
        public static string CheckStego(string filePath)
        {
            var output = new StringBuilder();
            try
            {
                output.AppendLine($"=== Stego Analysis of {Path.GetFileName(filePath)} ===");
                output.AppendLine();

                byte[] fileBytes = File.ReadAllBytes(filePath);

                // 🔥 1. Базовая инфа о файле
                output.AppendLine("=== File Info ===");
                output.AppendLine($"File Name: {Path.GetFileName(filePath)}");
                output.AppendLine($"File Size: {fileBytes.Length} bytes");
                output.AppendLine($"SHA-256: {CalculateSHA256(filePath)}");
                output.AppendLine();

                // 🔥 2. HEX + ASCII дамп
                output.AppendLine("=== HEX + ASCII DUMP (first 2048 bytes) ===");
                output.AppendLine(GenerateHexDump(fileBytes, 2048));
                output.AppendLine();

                // 🔥 3. Энтропия + текстовая карта
                output.AppendLine(EntropyAnalyzer.AnalyzeEntropy(fileBytes));
                output.AppendLine(EntropyAnalyzer.GenerateTextMap(fileBytes));
                output.AppendLine();

                // 🔥 4. Стандартный поиск скрытых данных
                ScanForHiddenData(fileBytes, output);

                // 🔥 5. Мини-анализ LSB
                double lsbRate = AnalyzeLSBNoise(fileBytes);
                output.AppendLine();
                output.AppendLine($"Estimated LSB noise: {lsbRate:F2}%");
                if (lsbRate > 45.0)
                {
                    output.AppendLine("⚠️ High LSB noise detected! Possible hidden message.");
                }

                output.AppendLine();
            }
            catch (Exception ex)
            {
                output.AppendLine($"Error during stego analysis: {ex.Message}");
            }

            return output.ToString();
        }


        private static string CalculateSHA256(string filePath)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        private static void SearchSignature(byte[] data, byte[] signature, string name, StringBuilder output)
        {
            for (int i = 0; i < data.Length - signature.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < signature.Length; j++)
                {
                    if (data[i + j] != signature[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    output.AppendLine($"⚠ Possible {name} signature found at offset 0x{i:X}");
                }
            }
        }

        private static double AnalyzeLSBNoise(byte[] data)
        {
            if (data.Length == 0) return 0;

            int bitCount = 0;
            foreach (var b in data)
            {
                if ((b & 0x01) != 0) bitCount++;
            }

            double rate = (bitCount / (double)data.Length) * 100.0;
            return rate;
        }
        public static void SaveEmbeddedArchive(byte[] data, int offset, string type, int fileIndex)
        {
            string folder = "Extracted";
            Directory.CreateDirectory(folder);

            string ext = type.ToLower();
            string filename = Path.Combine(folder, $"extracted_{fileIndex}.{ext}");

            try
            {
                using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(data, offset, data.Length - offset);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving extracted archive: {ex.Message}");
            }
        }
        private static void ScanForHiddenData(byte[] data, StringBuilder output)
        {
            int minTextLength = 20;
            var asciiBuffer = new List<byte>();
            int fileIndex = 0;

            output.AppendLine("=== Scanning for hidden data... ===");

            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];

                if (b >= 32 && b <= 126)
                {
                    asciiBuffer.Add(b);
                }
                else
                {
                    if (asciiBuffer.Count >= minTextLength)
                    {
                        SaveTextChunk(asciiBuffer.ToArray(), fileIndex++);
                        output.AppendLine($"[+] ASCII text block extracted at index {fileIndex - 1}");
                    }
                    asciiBuffer.Clear();
                }

                if (i + 4 < data.Length &&
                    data[i] == 0x50 && data[i + 1] == 0x4B &&
                    data[i + 2] == 0x03 && data[i + 3] == 0x04)
                {
                    SaveBinaryChunk(data, i, fileIndex++, "zip");
                    output.AppendLine($"[+] Embedded ZIP archive found at 0x{i:X}!");
                }
            }

            if (asciiBuffer.Count >= minTextLength)
            {
                SaveTextChunk(asciiBuffer.ToArray(), fileIndex++);
            }
        }

        private static void SaveTextChunk(byte[] buffer, int index)
        {
            string folder = "Extracted";
            Directory.CreateDirectory(folder);

            string asciiText = Encoding.UTF8.GetString(buffer);

            StringBuilder output = new StringBuilder();
            FlagScanner.SearchInText(asciiText, output);
            Base64Scanner.SearchBase64InText(asciiText, output, index);

            if (output.Length > 0)
            {
                File.WriteAllText(Path.Combine(folder, $"ascii_text_{index}_found.txt"), asciiText + "\n\n" + output.ToString());
            }
            else
            {
                File.WriteAllText(Path.Combine(folder, $"ascii_text_{index}.txt"), asciiText);
            }
        }

        private static void SaveBinaryChunk(byte[] data, int start, int index, string ext)
        {
            string folder = "Extracted";
            Directory.CreateDirectory(folder);
            byte[] chunk = new byte[data.Length - start];
            Array.Copy(data, start, chunk, 0, chunk.Length);
            File.WriteAllBytes(Path.Combine(folder, $"found_archive_{index}.{ext}"), chunk);
        }
        private static string GenerateHexDump(byte[] data, int maxLength = 2048)
        {
            var sb = new StringBuilder();
            int length = Math.Min(data.Length, maxLength);

            for (int i = 0; i < length; i += 16)
            {
                sb.Append($"{i:X8}  ");

                for (int j = 0; j < 16; j++)
                {
                    if (i + j < length)
                        sb.Append($"{data[i + j]:X2} ");
                    else
                        sb.Append("   ");
                }

                sb.Append(" ");

                for (int j = 0; j < 16; j++)
                {
                    if (i + j < length)
                    {
                        char c = (char)data[i + j];
                        sb.Append(Char.IsControl(c) ? '.' : c);
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
