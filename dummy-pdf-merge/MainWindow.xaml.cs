using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace PDFMerge;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<FileItem> _files = new();
    private readonly PdfMerger _pdfMerger = new();

    public MainWindow()
    {
        InitializeComponent();
        InitializeEventHandlers();
        FileListBox.ItemsSource = _files;
    }

    private void InitializeEventHandlers()
    {
        DropZone.DragOver += DropZone_DragOver;
        DropZone.Drop += DropZone_Drop;
        _pdfMerger.ProgressChanged += PdfMerger_ProgressChanged;
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void DropZone_Drop(object sender, DragEventArgs e)
    {
        try
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                AddFiles(files);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error adding files: {ex.Message}");
        }
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Supported Files (*.pdf;*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.tif)|*.pdf;*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.tif|" +
                    "PDF Files (*.pdf)|*.pdf|" +
                    "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.tif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.tif|" +
                    "All Files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            AddFiles(openFileDialog.FileNames);
        }
    }

    private void AddFiles(string[] filePaths)
    {
        var validFiles = filePaths
            .Where(fp => FileItem.IsValidFile(fp) && File.Exists(fp))
            .Where(fp => !_files.Any(f => f.FilePath.Equals(fp, StringComparison.OrdinalIgnoreCase)))
            .Select(fp => new FileItem(fp))
            .ToList();

        if (!validFiles.Any())
        {
            ShowWarning("No valid files to add.");
            return;
        }

        foreach (var file in validFiles)
        {
            _files.Add(file);
        }

        UpdateUI();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        if (_files.Count == 0)
        {
            ShowWarning("No files to clear.");
            return;
        }

        var result = MessageBox.Show(
            $"Clear all {_files.Count} file(s)?",
            "Confirm Clear",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _files.Clear();
            UpdateUI();
        }
    }

    private void MoveUpButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is FileItem file)
        {
            var index = _files.IndexOf(file);
            if (index > 0)
            {
                _files.Move(index, index - 1);
            }
        }
    }

    private void MoveDownButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is FileItem file)
        {
            var index = _files.IndexOf(file);
            if (index < _files.Count - 1)
            {
                _files.Move(index, index + 1);
            }
        }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is FileItem file)
        {
            _files.Remove(file);
            UpdateUI();
        }
    }

    private async void MergeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_files.Count == 0)
        {
            ShowWarning("Please add files before merging.");
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
            DefaultExt = ".pdf",
            FileName = $"output_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
        };

        if (saveFileDialog.ShowDialog() != true)
            return;

        try
        {
            DisableUI();
            StatusTextBlock.Text = "Starting merge...";
            ProgressBar.Visibility = Visibility.Visible;

            var filesList = _files.ToList();
            await _pdfMerger.MergePdfsAsync(filesList, saveFileDialog.FileName);

            ShowSuccess($"PDF merged successfully!\nSaved to: {saveFileDialog.FileName}");
            _files.Clear();
            UpdateUI();
        }
        catch (Exception ex)
        {
            ShowError($"Error merging PDF: {ex}");
        }
        finally
        {
            EnableUI();
            ProgressBar.Visibility = Visibility.Collapsed;
        }
    }

    private void PdfMerger_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            ProgressBar.Value = e.ProgressPercentage;
            StatusTextBlock.Text = e.Message;
        });
    }

    private void UpdateUI()
    {
        if (_files.Count == 0)
        {
            EmptyStatePanel.Visibility = Visibility.Visible;
            FileListBox.Visibility = Visibility.Collapsed;
            MergeButton.IsEnabled = false;
        }
        else
        {
            EmptyStatePanel.Visibility = Visibility.Collapsed;
            FileListBox.Visibility = Visibility.Visible;
            MergeButton.IsEnabled = true;
        }

        StatusTextBlock.Text = _files.Count == 0 ? "Ready" : $"{_files.Count} file(s) added";
    }

    private void DisableUI()
    {
        BrowseButton.IsEnabled = false;
        ClearButton.IsEnabled = false;
        MergeButton.IsEnabled = false;
        FileListBox.IsEnabled = false;
        DropZone.AllowDrop = false;
    }

    private void EnableUI()
    {
        BrowseButton.IsEnabled = true;
        ClearButton.IsEnabled = true;
        MergeButton.IsEnabled = _files.Count > 0;
        FileListBox.IsEnabled = true;
        DropZone.AllowDrop = true;
    }

    private void ShowWarning(string message)
    {
        MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void ShowSuccess(string message)
    {
        MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}