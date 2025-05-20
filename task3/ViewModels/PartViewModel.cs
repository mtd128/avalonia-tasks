using System;
using CommunityToolkit.Mvvm.ComponentModel;
using task3.Models;

namespace task3.ViewModels;

public partial class PartViewModel : ObservableObject
{
    private readonly Part _part;

    public Guid Id => _part.Id;

    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y; 

    public double Width => _part.Width;
    public double Height => _part.Height;

    public PartViewModel(Part part)
    {
        _part = part;
    }

    public void UpdatePosition(double conveyorX, double conveyorY, double partRelativeX)
    {
        X = conveyorX + partRelativeX;
        Y = conveyorY + (50 - Height) / 2;
    }
}