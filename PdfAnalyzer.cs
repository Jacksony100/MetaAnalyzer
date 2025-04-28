using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Meta
{
    public static class PdfAnalyzer
    {
        public static string ExtractPdfMetadata(string filePath)
        {
            var output = new StringBuilder();
            try
            {
                output.AppendLine("=== PDF Metadata ===");

                using (PdfDocument document = PdfReader.Open(filePath, PdfDocumentOpenMode.ReadOnly))
                {
                    output.AppendLine($"Title: {document.Info.Title}");
                    output.AppendLine($"Author: {document.Info.Author}");
                    output.AppendLine($"Subject: {document.Info.Subject}");
                    output.AppendLine($"Keywords: {document.Info.Keywords}");
                    output.AppendLine($"Creation Date: {document.Info.CreationDate}");
                    output.AppendLine($"Modification Date: {document.Info.ModificationDate}");

                    output.AppendLine();

                    int pageIndex = 0;
                    foreach (var page in document.Pages)
                    {
                        if (page.Contents.Elements.Count > 10)
                        {
                            output.AppendLine($"⚠ Page {pageIndex + 1}: unusually many contents streams ({page.Contents.Elements.Count})");
                        }
                        pageIndex++;
                    }
                    if (document.Internals.Catalog.Elements.ContainsKey("/Names"))
                    {
                        var names = document.Internals.Catalog.Elements["/Names"];
                        if (names != null && names.ToString().Contains("/EmbeddedFiles"))
                        {
                            output.AppendLine("⚠ Embedded files detected inside PDF!");
                        }
                    }
                }

                output.AppendLine();
            }
            catch (System.Exception ex)
            {
                output.AppendLine($"Error reading PDF metadata: {ex.Message}");
            }

            return output.ToString();
        }
    }
}
