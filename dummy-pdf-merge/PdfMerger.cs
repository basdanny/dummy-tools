using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PDFMerge;

public class PdfMerger
{
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

    public async Task<bool> MergePdfsAsync(List<FileItem> files, string outputPath)
    {
        try
        {
            if (!files.Any())
                throw new ArgumentException("No files to merge");

            var mergedPdf = new PdfDocument();
            var totalFiles = files.Count;

            for (int i = 0; i < totalFiles; i++)
            {
                var file = files[i];

                try
                {
                    if (file.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        AddPdfPages(mergedPdf, file.FilePath);
                    }
                    else
                    {
                        ConvertImageToPdfAndAdd(mergedPdf, file.FilePath);
                    }

                    ProgressChanged?.Invoke(this, new ProgressChangedEventArgs((i + 1) * 100 / totalFiles, $"Processing: {file.FileName}"));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error processing file {file.FileName}: {ex.Message}", ex);
                }
            }

            // Save the merged PDF
            await Task.Run(() => mergedPdf.Save(outputPath));
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(100, "Merge completed successfully!"));

            return true;
        }
        catch (Exception ex)
        {
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(0, $"Error: {ex.Message}"));
            throw;
        }
    }

    private void AddPdfPages(PdfDocument targetPdf, string sourcePdfPath)
    {
        try
        {
            using (var sourcePdf = PdfReader.Open(sourcePdfPath, PdfDocumentOpenMode.Import))
            {
                for (int pageIndex = 0; pageIndex < sourcePdf.PageCount; pageIndex++)
                {
                    var page = sourcePdf.Pages[pageIndex];
                    targetPdf.AddPage(page);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to add PDF pages from {sourcePdfPath}: {ex.Message}", ex);
        }
    }

    private void ConvertImageToPdfAndAdd(PdfDocument targetPdf, string imagePath)
    {
        try
        {
            using (var xImage = XImage.FromFile(imagePath))
            {
                var page = targetPdf.AddPage();
                page.Width = XUnit.FromPoint(xImage.PointWidth);
                page.Height = XUnit.FromPoint(xImage.PointHeight);

                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    gfx.DrawImage(xImage, 0, 0, page.Width.Point, page.Height.Point);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to convert image {imagePath} to PDF: {ex.Message}", ex);
        }
    }
}

public class ProgressChangedEventArgs : EventArgs
{
    public int ProgressPercentage { get; }
    public string Message { get; }

    public ProgressChangedEventArgs(int percentage, string message)
    {
        ProgressPercentage = percentage;
        Message = message;
    }
}