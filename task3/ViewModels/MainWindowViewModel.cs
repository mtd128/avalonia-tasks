using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using task3.Models;

namespace task3.ViewModels;

public partial class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<task3.ViewModels.ConveyorViewModel> Conveyors { get; } = new ObservableCollection<task3.ViewModels.ConveyorViewModel>();
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();

        private int _conveyorCounter = 0;
        private Loader _sharedLoader;
        private IMechanic _sharedMechanic;

        public MainWindowViewModel()
        {
            _sharedLoader = new Loader("Погрузчик Василий", loadAmount: 50);
            _sharedMechanic = new Mechanic("Механик Петрович");

            // Добавим пару конвейеров для старта
            AddConveyor();
            AddConveyor();
        }

        [RelayCommand]
        private void AddConveyor()
        {
            _conveyorCounter++;
            var conveyorName = $"Конвейер {_conveyorCounter}";
            var newCoreConveyor = new Conveyor(
                name: conveyorName,
                length: 300,
                speed: 50,
                maxMaterialCapacity: 100,
                initialMaterials: 70,
                lowMaterialThreshold: 20,
                breakdownProbabilityPerTick: 0.001,
                materialConsumptionPerPart: 1,
                partGenerationIntervalSeconds: 1.5
            );

            // Рассчитываем позицию нового конвейера
            double newConveyorY = Conveyors.Count * 80 + 20;
            
            var conveyorVM = new task3.ViewModels.ConveyorViewModel(newCoreConveyor, _sharedLoader, _sharedMechanic, 20, newConveyorY, Log);
            Conveyors.Add(conveyorVM);
            conveyorVM.TogglePauseResume();
            Log($"Добавлен и запущен {conveyorName}");
        }

        [RelayCommand]
        private void RemoveLastConveyor()
        {
            if (Conveyors.Count > 0)
            {
                var conveyorToRemove = Conveyors[Conveyors.Count - 1];
                Log($"Удаляется {conveyorToRemove.Name}");
                conveyorToRemove.Dispose();
                Conveyors.RemoveAt(Conveyors.Count - 1);
            }
        }

        private void Log(string message)
        {
            // Всегда логируем в UI потоке
            Dispatcher.UIThread.Post(() =>
            {
                LogMessages.Insert(0, $"{DateTime.Now:HH:mm:ss} - {message}");
                if (LogMessages.Count > 100)
                {
                    LogMessages.RemoveAt(LogMessages.Count - 1);
                }
            });
        }

        public void Cleanup()
        {
            foreach (var conveyorVM in Conveyors)
            {
                conveyorVM.Dispose();
            }
            Conveyors.Clear();
            LogMessages.Clear();
        }
    }