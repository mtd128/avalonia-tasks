using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace task2.Models;

public class ReflectionModel
{
    public string? AssemblyPath { get; set; }
    public List<Type>? LoadedTypes { get; set; }
    public Type? SelectedType { get; set; }
    public MethodInfo? SelectedMethod { get; set; }
    public object? Instance { get; set; }
    public List<ParameterInfo>? MethodParameters { get; set; }
    public object?[]? ParameterValues { get; set; }
    public object? MethodResult { get; set; }

    public bool LoadAssembly(string path)
    {
        try
        {
            Assembly assembly = Assembly.LoadFrom(path);
            LoadedTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .ToList();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void SelectType(Type type)
    {
        SelectedType = type;
        Instance = Activator.CreateInstance(type);
    }

    public void SelectMethod(MethodInfo method)
    {
        SelectedMethod = method;
        MethodParameters = method.GetParameters().ToList();
        ParameterValues = new object[MethodParameters.Count];
    }

    public bool ExecuteMethod()
    {
        if (SelectedMethod == null || Instance == null || ParameterValues == null)
            return false;

        try
        {
            MethodResult = SelectedMethod.Invoke(Instance, ParameterValues);
            return true;
        }
        catch
        {
            return false;
        }
    }
} 