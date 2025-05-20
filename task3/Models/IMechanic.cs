using System;
using System.Threading.Tasks;

namespace task3.Models;

public interface IMechanic
{
    string Name { get; }
    Task FixConveyorAsync(Conveyor conveyor, Action<string> logAction);
}