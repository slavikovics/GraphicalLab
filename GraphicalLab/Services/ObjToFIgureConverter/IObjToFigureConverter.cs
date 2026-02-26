using System.Threading.Tasks;
using GraphicalLab.Points;

namespace GraphicalLab.Services.ObjToFigureConverter;

public interface IObjToFigureConverter
{
    Task<Figure> ConvertFromFileAsync(string filePath);
}