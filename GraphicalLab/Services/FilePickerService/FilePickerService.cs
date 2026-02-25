using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace GraphicalLab.Services.FilePickerService;

public class FilePickerService : IFilePickerService
{
    private readonly Window _targetWindow;

    public FilePickerService(Window targetWindow)
    {
        _targetWindow = targetWindow ?? throw new ArgumentNullException(nameof(targetWindow));
    }

    public async Task<IStorageFile?> PickAsync()
    {
        var topLevel = TopLevel.GetTopLevel(_targetWindow);
        if (topLevel == null)
        {
            throw new InvalidOperationException("Could not get top level window.");
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Files",
            AllowMultiple = true,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new("All Files")
                {
                    Patterns = ["*.*"]
                }
            }
        });

        return files.FirstOrDefault();
    }
}