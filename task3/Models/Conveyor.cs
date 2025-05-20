using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace task3.Models
{
    public class Conveyor : IDisposable
    {
        public Guid Id { get; }
        public string Name { get; set; }

        private List<Part> _partsOnBelt;
        private readonly object _partsLock = new object();

        public double Length { get; }
        public double Speed { get; set; }
        public int MaxMaterialCapacity { get; }
        private int _currentMaterials;
        public int LowMaterialThreshold { get; }

        private bool _isBroken;
        private bool _isRunning;
        private readonly double _breakdownProbabilityPerTick;
        private readonly int _materialConsumptionPerPart;
        private readonly double _partGenerationIntervalSeconds;

        private CancellationTokenSource _cts;
        private Task _simulationTask;
        private DateTime _lastPartGeneratedTime;

        // События
        public event EventHandler<ConveyorEventArgs> MaterialsLow;
        public event EventHandler<ConveyorEventArgs> BrokeDown;
        public event EventHandler<ConveyorEventArgs> StateChanged;

        public int CurrentMaterials
        {
            get => _currentMaterials;
            private set
            {
                if (_currentMaterials != value)
                {
                    _currentMaterials = value;
                    OnStateChanged("Материалы обновлены");
                }
            }
        }

        public bool IsBroken
        {
            get => _isBroken;
            private set
            {
                if (_isBroken != value)
                {
                    _isBroken = value;
                    OnStateChanged(value ? "Сломан" : "Исправен");
                }
            }
        }
        public bool IsRunning => _isRunning;

        public ReadOnlyCollection<Part> PartsOnBelt
        {
            get
            {
                lock (_partsLock)
                {
                    return new ReadOnlyCollection<Part>(new List<Part>(_partsOnBelt));
                }
            }
        }

        public Conveyor(string name, double length, double speed, int maxMaterialCapacity, int initialMaterials,
                        int lowMaterialThreshold, double breakdownProbabilityPerTick,
                        int materialConsumptionPerPart = 1, double partGenerationIntervalSeconds = 2.0)
        {
            Id = Guid.NewGuid();
            Name = name;
            Length = length;
            Speed = speed;
            MaxMaterialCapacity = maxMaterialCapacity;
            _currentMaterials = Math.Min(initialMaterials, maxMaterialCapacity);
            LowMaterialThreshold = lowMaterialThreshold;
            _breakdownProbabilityPerTick = breakdownProbabilityPerTick;
            _materialConsumptionPerPart = materialConsumptionPerPart;
            _partGenerationIntervalSeconds = partGenerationIntervalSeconds;

            _partsOnBelt = new List<Part>();
            _isBroken = false;
            _isRunning = false;
            _lastPartGeneratedTime = DateTime.MinValue;
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            if (IsBroken) {
                 OnStateChanged("Попытка запуска сломанного конвейера");
                 _isRunning = false; // Не запускаем
                 return;
            }

            _cts = new CancellationTokenSource();
            _simulationTask = Task.Run(async () => await SimulateAsync(_cts.Token), _cts.Token);
            OnStateChanged("Запущен");
            Debug.WriteLine($"Conveyor {Name} ({Id}): Started simulation task.");
        }

        public void Stop()
        {
            if (!_isRunning && (_simulationTask == null || _simulationTask.IsCompleted))
            {
                Debug.WriteLine($"Conveyor {Name} ({Id}): Stop called, but already stopped or task null/completed.");
                if (_isRunning) _isRunning = false;
                if (_cts != null) 
                { 
                    _cts.Dispose(); 
                    _cts = null; 
                }
                if (_simulationTask != null && _simulationTask.IsCompleted) 
                { 
                    _simulationTask = null; 
                }
                return;
            }

            _isRunning = false;
            OnStateChanged("Останавливается...");
            Debug.WriteLine($"Conveyor {Name} ({Id}): Stop initiated. IsRunning set to false.");

            if (_cts != null && !_cts.IsCancellationRequested)
            {
                Debug.WriteLine($"Conveyor {Name} ({Id}): Requesting cancellation via CancellationTokenSource.");
                try
                {
                    _cts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    Debug.WriteLine($"Conveyor {Name} ({Id}): Attempted to cancel a disposed CancellationTokenSource.");
                }
            }

            if (_simulationTask != null && !_simulationTask.IsCompleted)
            {
                Debug.WriteLine($"Conveyor {Name} ({Id}): Waiting for simulation task to complete...");
                try
                {
                    if (!_simulationTask.Wait(TimeSpan.FromMilliseconds(1000)))
                    {
                        Debug.WriteLine($"Warning: Conveyor {Name} ({Id}) simulation task did not complete within timeout after cancellation signal.");
                    }
                    else
                    {
                        Debug.WriteLine($"Conveyor {Name} ({Id}): Simulation task completed after cancellation.");
                    }
                }
                catch (AggregateException ae)
                {
                    Debug.WriteLine($"Conveyor {Name} ({Id}): AggregateException during task wait: {ae.Flatten().InnerExceptions.FirstOrDefault()?.GetType().Name}");
                    ae.Handle(ex => ex is TaskCanceledException || ex is OperationCanceledException);
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"Conveyor {Name} ({Id}): Simulation task was canceled (TaskCanceledException).");
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine($"Conveyor {Name} ({Id}): Simulation task was canceled (OperationCanceledException).");
                }
            }
            _simulationTask = null; // Обнуляем ссылку в любом случае после попытки ожидания или если он был null/завершен
            Debug.WriteLine($"Conveyor {Name} ({Id}): Simulation task reference set to null.");


            if (_cts != null)
            {
                try
                {
                    _cts.Dispose();
                }
                catch (ObjectDisposedException)
                {
                }
                _cts = null;
                Debug.WriteLine($"Conveyor {Name} ({Id}): CancellationTokenSource disposed and set to null.");
            }
            
            OnStateChanged("Остановлен");
            Debug.WriteLine($"Conveyor {Name} ({Id}): Stop process completed.");
        }


        private async Task SimulateAsync(CancellationToken token)
        {
            DateTime lastTickTime = DateTime.UtcNow;
            Random random = new Random(Guid.NewGuid().GetHashCode()); // Более уникальный Random для каждого потока

            Debug.WriteLine($"Conveyor {Name} ({Id}): Simulation loop started.");
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    if (IsBroken || !_isRunning)
                    {
                        await Task.Delay(100, token);
                        lastTickTime = DateTime.UtcNow;
                        continue;
                    }

                    DateTime currentTickTime = DateTime.UtcNow;
                    double deltaTimeSeconds = (currentTickTime - lastTickTime).TotalSeconds;
                    lastTickTime = currentTickTime;

                    if (deltaTimeSeconds <= 0)
                    {
                         await Task.Delay(10, token);
                         continue;
                    }

                    MoveParts(deltaTimeSeconds);
                    TryGeneratePart();
                    
                    if (Speed > 0 && random.NextDouble() < (_breakdownProbabilityPerTick * deltaTimeSeconds * Speed * 0.1) )
                    {
                        BreakDown();
                    }

                    if (CurrentMaterials <= LowMaterialThreshold && CurrentMaterials > 0)
                    {
                        OnMaterialsLow();
                    }
                    else if (CurrentMaterials == 0 && _partsOnBelt.Any())
                    {
                        OnStateChanged("Материалы закончились, но детали еще движутся");
                    }
                    else if (CurrentMaterials == 0 && !_partsOnBelt.Any())
                    {
                        OnStateChanged("Материалы закончились, конвейер пуст");
                    }
                    
                    OnStateChanged("Тик симуляции");

                    await Task.Delay(30, token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"Conveyor {Name} ({Id}): Simulation loop gracefully canceled via OperationCanceledException.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Conveyor {Name} ({Id}) simulation: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                IsBroken = true;
                OnBrokeDown();
            }
            finally
            {
                Debug.WriteLine($"Conveyor {Name} ({Id}): Simulation loop finished.");
                _isRunning = false; 
            }
        }

        private void MoveParts(double deltaTimeSeconds)
        {
            if (deltaTimeSeconds <= 0) return;
            lock (_partsLock)
            {
                List<Part> partsToRemove = new List<Part>();
                foreach (var part in _partsOnBelt)
                {
                    part.CurrentPositionX += Speed * deltaTimeSeconds;
                    if (part.CurrentPositionX >= Length)
                    {
                        partsToRemove.Add(part);
                    }
                }
                if (partsToRemove.Any())
                {
                    partsToRemove.ForEach(p => _partsOnBelt.Remove(p));
                    OnStateChanged($"{partsToRemove.Count} деталей сошло с конвейера");
                }
            }
        }

        private void TryGeneratePart()
        {
            if (Speed <= 0 || IsBroken) return;

            if ((DateTime.UtcNow - _lastPartGeneratedTime).TotalSeconds >= _partGenerationIntervalSeconds)
            {
                if (CurrentMaterials >= _materialConsumptionPerPart)
                {
                    lock (_partsLock)
                    {
                        if (!_partsOnBelt.Any() || _partsOnBelt.Last().CurrentPositionX > 25)
                        {
                            var newPart = new Part(width: 20, height: 10, initialPositionX: 0);
                            _partsOnBelt.Add(newPart);
                            CurrentMaterials -= _materialConsumptionPerPart;
                            _lastPartGeneratedTime = DateTime.UtcNow;
                            OnStateChanged("Деталь создана");
                        }
                    }
                }
            }
        }
        
        public void AddMaterials(int amount)
        {
            if (amount <= 0) return;
            CurrentMaterials = Math.Min(CurrentMaterials + amount, MaxMaterialCapacity);
        }

        private void BreakDown()
        {
            if (IsBroken) return;
            IsBroken = true;
            OnBrokeDown();
        }

        public void SetFixed()
        {
            if (!IsBroken) return; // Уже исправен
            IsBroken = false;
        }

        protected virtual void OnMaterialsLow()
        {
            MaterialsLow?.Invoke(this, new ConveyorEventArgs(this, "Низкий уровень материалов"));
        }

        protected virtual void OnBrokeDown()
        {
            BrokeDown?.Invoke(this, new ConveyorEventArgs(this, "Конвейер сломался"));
        }
        
        protected virtual void OnStateChanged(string message)
        {
            StateChanged?.Invoke(this, new ConveyorEventArgs(this, message));
        }

        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            Debug.WriteLine($"Conveyor {Name} ({Id}): Dispose({disposing}) called.");
            if (disposing)
            {
                MaterialsLow = null;
                BrokeDown = null;
                StateChanged = null;

                Stop();

                lock(_partsLock)
                {
                    _partsOnBelt.Clear();
                }
            }

            _disposed = true;
            Debug.WriteLine($"Conveyor {Name} ({Id}): Disposed.");
        }

        ~Conveyor()
        {
            Debug.WriteLine($"Conveyor {Name} ({Id}): Finalizer called.");
            Dispose(false);
        }
    }

    public class ConveyorEventArgs : EventArgs
    {
        public Conveyor Conveyor { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }

        public ConveyorEventArgs(Conveyor conveyor, string message)
        {
            Conveyor = conveyor;
            Message = message;
            Timestamp = DateTime.Now;
        }
    }
}