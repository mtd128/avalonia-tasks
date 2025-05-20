using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using task2.Models;

namespace task2.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ReflectionModel _reflectionModel = new();

    [ObservableProperty]
    private string? _assemblyPath;

    [ObservableProperty]
    private ObservableCollection<string> _loadedTypes = new();

    [ObservableProperty]
    private string? _selectedTypeName;

    [ObservableProperty]
    private ObservableCollection<string> _methods = new();

    [ObservableProperty]
    private string? _selectedMethodName;

    [ObservableProperty]
    private ObservableCollection<ParameterViewModel> _parameters = new();

    [ObservableProperty]
    private string? _methodResult;

    [RelayCommand]
    private void LoadAssembly()
    {
        if (string.IsNullOrEmpty(AssemblyPath)) return;

        if (_reflectionModel.LoadAssembly(AssemblyPath))
        {
            LoadedTypes.Clear();
            foreach (var type in _reflectionModel.LoadedTypes!)
            {
                LoadedTypes.Add(type.Name);
            }
        }
    }

    [RelayCommand]
    private void SelectType()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedTypeName)) return;

            var type = _reflectionModel.LoadedTypes!.FirstOrDefault(t => t.Name == SelectedTypeName);
            if (type == null) return;

            _reflectionModel.SelectType(type);
            Methods.Clear();
            
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                            .Where(m => !m.IsSpecialName && !m.IsConstructor)
                            .ToList();

            foreach (var method in methods)
            {
                Methods.Add(method.Name);
            }
        }
        catch (Exception ex)
        {
            MethodResult = $"Error selecting type: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SelectMethod()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedMethodName)) return;

            var method = _reflectionModel.SelectedType!.GetMethod(SelectedMethodName);
            if (method == null) return;

            _reflectionModel.SelectMethod(method);
            Parameters.Clear();
            foreach (var param in _reflectionModel.MethodParameters!)
            {
                Parameters.Add(new ParameterViewModel
                {
                    Name = param.Name!,
                    Type = param.ParameterType.Name,
                    Value = string.Empty
                });
            }
        }
        catch (Exception ex)
        {
            MethodResult = $"Error selecting method: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ExecuteMethod()
    {
        try
        {
            if (_reflectionModel.MethodParameters == null) return;

            for (int i = 0; i < Parameters.Count; i++)
            {
                var param = Parameters[i];
                var paramType = _reflectionModel.MethodParameters[i].ParameterType;
                try
                {
                    _reflectionModel.ParameterValues![i] = Convert.ChangeType(param.Value, paramType);
                }
                catch
                {
                    MethodResult = $"Error converting parameter '{param.Name}' to type {paramType.Name}";
                    return;
                }
            }

            if (_reflectionModel.ExecuteMethod())
            {
                MethodResult = _reflectionModel.MethodResult?.ToString() ?? "Method executed successfully";
            }
            else
            {
                MethodResult = "Error executing method";
            }
        }
        catch (Exception ex)
        {
            MethodResult = $"Error executing method: {ex.Message}";
        }
    }
}

public class ParameterViewModel : ViewModelBase
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}