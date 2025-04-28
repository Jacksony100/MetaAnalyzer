using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meta
{
    public static class EntropyAnalyzer
    {
        public static string GenerateTextMap(byte[] data, int windowSize = 1024)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== File Map Based on Entropy ===");

            for (int i = 0; i < data.Length; i += windowSize)
            {
                int size = Math.Min(windowSize, data.Length - i);
                double entropy = CalculateEntropy(data, i, size);

                string status = (entropy > 7.5) ? "High Entropy (Possibly Hidden Data)" : "Normal";

                sb.AppendLine($"0x{i:X8} - 0x{i + size - 1:X8}: {status}");
            }

            return sb.ToString();
        }
        public static string AnalyzeEntropy(byte[] data, int windowSize = 1024)
        {
            var output = new StringBuilder();
            output.AppendLine("=== Entropy Analysis ===");

            List<double> entropyValues = new List<double>();
            for (int i = 0; i < data.Length; i += windowSize)
            {
                int size = Math.Min(windowSize, data.Length - i);
                double entropy = CalculateEntropy(data, i, size);
                entropyValues.Add(entropy);

                if (entropy > 7.5)
                {
                    output.AppendLine($"⚠️ High entropy region at 0x{i:X} (Entropy = {entropy:F2})");
                }
            }

            output.AppendLine();
            return output.ToString();
        }

        private static double CalculateEntropy(byte[] data, int offset, int length)
        {
            if (length == 0) return 0;

            int[] counts = new int[256];
            for (int i = 0; i < length; i++)
            {
                counts[data[offset + i]]++;
            }

            double entropy = 0.0;
            for (int i = 0; i < 256; i++)
            {
                if (counts[i] == 0) continue;

                double p = (double)counts[i] / length;
                entropy -= p * Math.Log(p, 2);
            }

            return entropy;
        }
    }
}
