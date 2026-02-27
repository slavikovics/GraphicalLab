using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GraphicalLab.Points;
using GraphicalLab.Services.FilePickerService;
using GraphicalLab.Services.ObjToFigureConverter;

namespace GraphicalLab.Services.FigureLoaderService;

public class FigureLoader : IFigureLoader
{
    private readonly IFilePickerService _filePickerService;
    private readonly IObjToFigureConverter _objToFigureConverter;

    public FigureLoader(IFilePickerService filePickerService, IObjToFigureConverter objToFigureConverter)
    {
        _filePickerService = filePickerService;
        _objToFigureConverter = objToFigureConverter;
    }

    public async Task<Figure> ProcessJson(string path)
    {
        var text = await File.ReadAllTextAsync(path);
        var figure = JsonSerializer.Deserialize<Figure>(text);
        if (figure == null) throw new JsonException();
        return figure;
    }

    public async Task<Figure> LoadFigure()
    {
        var file = await _filePickerService.PickAsync();
        var path = file?.Path.LocalPath;
        if (file == null || path == null) throw new FileNotFoundException();
            
        switch (file.Name.Split(".")[1])
        {
            case "json": return await ProcessJson(path);
            case "obj": return await _objToFigureConverter.ConvertFromFileAsync(path);
            default: throw new FileLoadException("File extension not implemented", path);
        }
    }
}