using System;
using System.Threading.Tasks;

namespace task3.Models;

public class Mechanic : IMechanic
{
    public string Name { get; }
    private readonly int _minFixTimeMs;
    private readonly int _maxFixTimeMs;

    public Mechanic(string name, int minFixTimeMs = 2000, int maxFixTimeMs = 5000)
    {
        Name = name;
        _minFixTimeMs = minFixTimeMs;
        _maxFixTimeMs = maxFixTimeMs;
    }

    public async Task FixConveyorAsync(Conveyor conveyor, Action<string> logAction)
    {
        logAction($"{Name} (Механик): Начинаю ремонт конвейера {conveyor.Id}.");
        await Task.Delay(new Random().Next(_minFixTimeMs, _maxFixTimeMs));
        conveyor.SetFixed();
        logAction($"{Name} (Механик): Конвейер {conveyor.Id} отремонтирован.");
    }
}