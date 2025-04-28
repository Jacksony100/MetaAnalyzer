using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Meta
{
    public static class FlagScanner
    {
        public static List<string> Patterns = new List<string>();

        public static void LoadPatterns(string input)
        {
            Patterns.Clear();
            string[] parts = input.Split(',');
            foreach (var part in parts)
            {
                Patterns.Add(part.Trim());
            }
        }

        public static void SearchInText(string text, StringBuilder output)
        {
            foreach (var pattern in Patterns)
            {
                if (string.IsNullOrWhiteSpace(pattern)) continue;

                if (text.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    output.AppendLine($"[!] Pattern match found: \"{pattern}\" in text!");
                }
                if (pattern.Contains("{"))
                {
                    string regexPattern = Regex.Escape(pattern.Substring(0, pattern.IndexOf("{"))) + @"\{.*?\}";
                    var matches = Regex.Matches(text, regexPattern);
                    foreach (Match match in matches)
                    {
                        output.AppendLine($"[!] Full flag detected: {match.Value}");
                    }
                }
            }
        }
    }
}
