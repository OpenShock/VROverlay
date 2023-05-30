using System;
// ReSharper disable once ClassNeverInstantiated.Global
public class Control
{
    public string Id { get; set; }
    public ControlType Type { get; set; }
    public byte Intensity { get; set; }
    public uint Duration { get; set; }
}