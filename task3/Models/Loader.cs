using System;
using System.Threading.Tasks;

namespace task3.Models;

public class Loader
{
    public string Name { get; }
    private readonly int _minLoadTimeMs;
    private readonly int _maxLoadTimeMs;
    private readonly int _loadAmount;

    public Loader(string name, int loadAmount, int minLoadTimeMs = 1000, int maxLoadTimeMs = 3000)
    {
        Name = name;
        _loadAmount = loadAmount;
        _minLoadTimeMs = minLoadTimeMs;
        _maxLoadTimeMs = maxLoadTimeMs;
    }

    public async Task LoadMaterialsAsync(Conveyor conveyor, Action<string> logAction)
    {
        logAction($"{Name} (Погрузчик): Загружаю {_loadAmount} материалов на конвейер {conveyor.Id}.");
        await Task.Delay(new Random().Next(_minLoadTimeMs, _maxLoadTimeMs));
        conveyor.AddMaterials(_loadAmount);
        logAction($"{Name} (Погрузчик): {_loadAmount} материалов загружено на конвейер {conveyor.Id}.");
    }
}