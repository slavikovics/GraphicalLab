using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GraphicalLab.Points;
using GraphicalLab.Services.FilePickerService;

namespace GraphicalLab.Services.FigureLoaderService;

public class FigureLoader : IFigureLoader
{
    private readonly IFilePickerService _filePickerService;

    public FigureLoader(IFilePickerService filePickerService)
    {
        _filePickerService = filePickerService;
    }

    public async Task<Figure> LoadFigure()
    {
        try
        {
            var file = await _filePickerService.PickAsync();
            var path = file?.Path;

            if (path != null)
            {
                var text = await File.ReadAllTextAsync(path.ToString());
                var figure = JsonSerializer.Deserialize<Figure>(text);
                if (figure == null) throw new JsonException();
                return figure;
            }

            throw new FileNotFoundException();
        }
        catch (Exception e)
        {
            throw new FileLoadException("Failed to load figure", e);
        }
    }
}