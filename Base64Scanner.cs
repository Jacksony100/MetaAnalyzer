using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace Meta
{
    public static class Base64Scanner
    {
        public static void SearchBase64InText(string text, StringBuilder output, int asciiIndex)
        {
            var matches = Regex.Matches(text, @"(?:[A-Za-z0-9+/]{4}){3,}(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?");

            if (matches.Count == 0)
                return;

            string folder = "Extracted";
            Directory.CreateDirectory(folder);

            int foundCount = 0;

            foreach (Match match in matches)
            {
                try
                {
                    string base64string = match.Value;
                    byte[] data = Convert.FromBase64String(base64string);
                    string decodedText = Encoding.UTF8.GetString(data);

                    if (string.IsNullOrWhiteSpace(decodedText)) continue;

                    string decodedFile = Path.Combine(folder, $"ascii_text_{asciiIndex}_base64_{foundCount}.txt");
                    File.WriteAllText(decodedFile, decodedText);

                    output.AppendLine($"[+] Decoded Base64 block saved at: {decodedFile}");
                    FlagScanner.SearchInText(decodedText, output);

                    foundCount++;
                }
                catch
                {
                }
            }
        }
    }
}
