using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using task3.Models;

namespace task3.ViewModels;

public partial class ConveyorViewModel : ObservableObject, IDisposable
    {
        private readonly Conveyor _conveyor;
        private readonly Loader _loader;
        private readonly IMechanic _mechanic;
        private readonly Action<string> _logAction;

        public Guid Id => _conveyor.Id;
        public string Name => _conveyor.Name;

        [ObservableProperty]
        private double _canvasX;

        [ObservableProperty]
        private double _canvasY;

        public double Length => _conveyor.Length;
        public double Height { get; } = 50;

        [ObservableProperty]
        private int _currentMaterials;

        [ObservableProperty]
        private bool _isBroken;
        
        [ObservableProperty]
        private bool _isRunning;

        [ObservableProperty]
        private string _statusMessage;

        public int MaxMaterialCapacity => _conveyor.MaxMaterialCapacity;
        public ObservableCollection<PartViewModel> DisplayParts { get; } = new ObservableCollection<PartViewModel>();

        private bool _isLoadingMaterials = false;
        private bool _isBeingFixed = false;

        public ConveyorViewModel(Conveyor conveyor, Loader loader, IMechanic mechanic, double canvasX, double canvasY, Action<string> logAction)
        {
            _conveyor = conveyor;
            _loader = loader;
            _mechanic = mechanic;
            _logAction = logAction;

            CanvasX = canvasX;
            CanvasY = canvasY;

            _conveyor.MaterialsLow += OnConveyorMaterialsLow;
            _conveyor.BrokeDown += OnConveyorBrokeDown;
            _conveyor.StateChanged += OnConveyorStateChanged;

            UpdateFromModel("Инициализация");
        }

        private void UpdateFromModel(string messageContext)
        {
            CurrentMaterials = _conveyor.CurrentMaterials;
            IsBroken = _conveyor.IsBroken;
            IsRunning = _conveyor.IsRunning;
            StatusMessage = $"[{_conveyor.Name} - {messageContext}]: Мат: {CurrentMaterials}/{MaxMaterialCapacity}, {(IsBroken ? "СЛОМАН" : $"OK, {(IsRunning ? "РАБОТАЕТ" : "СТОП")}")}";

            var currentModelParts = _conveyor.PartsOnBelt.ToList(); // Копия для безопасной итерации
            var currentDisplayPartIds = DisplayParts.Select(pvm => pvm.Id).ToList();
            var modelPartIds = currentModelParts.Select(p => p.Id).ToList();

            var partsToRemove = DisplayParts.Where(pvm => !modelPartIds.Contains(pvm.Id)).ToList();
            foreach (var pvmToRemove in partsToRemove)
            {
                DisplayParts.Remove(pvmToRemove);
            }

            foreach (var modelPart in currentModelParts)
            {
                var existingPvm = DisplayParts.FirstOrDefault(pvm => pvm.Id == modelPart.Id);
                if (existingPvm != null)
                {
                    existingPvm.UpdatePosition(CanvasX, CanvasY, modelPart.CurrentPositionX);
                }
                else
                {
                    var newPvm = new PartViewModel(modelPart);
                    newPvm.UpdatePosition(CanvasX, CanvasY, modelPart.CurrentPositionX);
                    DisplayParts.Add(newPvm);
                }
            }
        }
        
        private void OnConveyorStateChanged(object sender, ConveyorEventArgs e)
        {
            Dispatcher.UIThread.Post(() => UpdateFromModel(e.Message));
        }

        private async void OnConveyorMaterialsLow(object sender, ConveyorEventArgs e)
        {
            // Запускаем на UI потоке для безопасного доступа к _isLoadingMaterials
            await Dispatcher.UIThread.InvokeAsync((Func<Task>)(async () =>
            {
                if (_isLoadingMaterials || _conveyor.CurrentMaterials >= _conveyor.LowMaterialThreshold || _conveyor.IsBroken) return; // Уже грузится, уже достаточно или сломан

                _isLoadingMaterials = true;
                _logAction($"Конвейер {Name}: Мало материалов. Погрузчик {_loader.Name} вызван.");
                StatusMessage = $"{Name}: Погрузчик в пути...";

                await Task.Run(async () => await _loader.LoadMaterialsAsync(_conveyor, _logAction));
                
                UpdateFromModel("Материалы загружены");
                _logAction($"Конвейер {Name}: Погрузчик {_loader.Name} завершил загрузку.");
                _isLoadingMaterials = false;
            }));
        }

        private async void OnConveyorBrokeDown(object sender, ConveyorEventArgs e)
        {
             await Dispatcher.UIThread.InvokeAsync((Func<Task>)(async () =>
             {
                 if (_isBeingFixed) return;

                 _isBeingFixed = true;
                 _logAction($"Конвейер {Name}: Сломался! Механик {_mechanic.Name} вызван.");
                 StatusMessage = $"{Name}: Механик в пути...";
                
                 await Task.Run(async () => await _mechanic.FixConveyorAsync(_conveyor, _logAction));

                 UpdateFromModel("Отремонтирован");
                 _logAction($"Конвейер {Name}: Механик {_mechanic.Name} завершил ремонт.");
                 _isBeingFixed = false;
             }));
        }

        [RelayCommand]
        public void TogglePauseResume()
        {
            if (_conveyor.IsRunning)
            {
                _conveyor.Stop();
            }
            else
            {
                _conveyor.Start();
            }
            UpdateFromModel(_conveyor.IsRunning ? "Запущен" : "Остановлен");
        }

        public void Dispose()
        {
            _conveyor.MaterialsLow -= OnConveyorMaterialsLow;
            _conveyor.BrokeDown -= OnConveyorBrokeDown;
            _conveyor.StateChanged -= OnConveyorStateChanged;
            _conveyor.Dispose();
            DisplayParts.Clear();
        }
    }