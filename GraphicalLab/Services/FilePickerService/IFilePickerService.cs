using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace GraphicalLab.Services.FilePickerService;

public interface IFilePickerService
{
    Task<IStorageFile?> PickAsync();
}