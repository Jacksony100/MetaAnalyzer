using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Drawing;

namespace Meta
{
    public static class LSBDecoder
    {
        public static void ExtractHiddenMessage(string imagePath)
        {
            string folder = "Extracted";
            Directory.CreateDirectory(folder);

            using (var bmp = new Bitmap(imagePath))
            {
                var bits = new System.Collections.Generic.List<byte>();

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color pixel = bmp.GetPixel(x, y);

                        bits.Add((byte)(pixel.R & 0x01));
                        bits.Add((byte)(pixel.G & 0x01));
                        bits.Add((byte)(pixel.B & 0x01));
                    }
                }

                var messageBytes = new System.Collections.Generic.List<byte>();
                for (int i = 0; i < bits.Count; i += 8)
                {
                    if (i + 8 <= bits.Count)
                    {
                        byte b = 0;
                        for (int j = 0; j < 8; j++)
                        {
                            b |= (byte)(bits[i + j] << (7 - j));
                        }
                        messageBytes.Add(b);
                    }
                }

                string message = Encoding.UTF8.GetString(messageBytes.ToArray());
                string savePath = Path.Combine(folder, $"lsb_hidden_message.txt");
                File.WriteAllText(savePath, message);

                Console.WriteLine($"[+] Hidden message extracted. Length: {message.Length} bytes");
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = savePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] Error opening extracted file: {ex.Message}");
                }
            }
        }
    }
}
