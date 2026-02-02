using System;
using System.IO;
using System.Linq;

namespace PDFMerge;

public class FileItem
{
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public string FileType { get; set; }
    public string SizeDisplay { get; set; }
    public long FileSize { get; set; }

    public FileItem(string filePath)
    {
        FilePath = filePath;
        var fileInfo = new FileInfo(filePath);
        FileName = fileInfo.Name;
        FileSize = fileInfo.Length;
        SizeDisplay = FormatFileSize(FileSize);
        FileType = DetermineFileType(filePath);
    }

    private static string DetermineFileType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        return extension switch
        {
            ".pdf" => "PDF Document",
            ".png" => "PNG Image",
            ".jpg" or ".jpeg" => "JPEG Image",
            ".bmp" => "Bitmap Image",
            ".gif" => "GIF Image",
            ".tiff" or ".tif" => "TIFF Image",
            _ => "Unknown File"
        };
    }

    private static string FormatFileSize(long bytes)
    {
        const long mb = 1024 * 1024;
        const long kb = 1024;

        if (bytes >= mb)
            return $"{bytes / (double)mb:F2} MB";
        if (bytes >= kb)
            return $"{bytes / (double)kb:F2} KB";
        return $"{bytes} B";
    }

    public static bool IsValidFile(string filePath)
    {
        var validExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".tif" };
        var extension = Path.GetExtension(filePath).ToLower();
        return validExtensions.Contains(extension);
    }
}