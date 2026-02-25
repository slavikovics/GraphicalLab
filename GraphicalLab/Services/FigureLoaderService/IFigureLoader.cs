using System.Threading.Tasks;
using GraphicalLab.Points;

namespace GraphicalLab.Services.FigureLoaderService;

public interface IFigureLoader
{
    Task<Figure> LoadFigure();
}