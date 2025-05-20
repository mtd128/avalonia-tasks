using System;

namespace task3.Models;

public class Part
{
    public Guid Id { get; }
    public double CurrentPositionX { get; set; }
    public double Width { get; }
    public double Height { get; }

    public Part(double width, double height, double initialPositionX = 0)
    {
        Id = Guid.NewGuid();
        Width = width;
        Height = height;
        CurrentPositionX = initialPositionX;
    }
}