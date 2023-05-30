// ReSharper disable once ClassNeverInstantiated.Global

public class ControlLog
{
    public GenericIn Shocker { get; set; }
    public ControlType Type { get; set; }
    public byte Intensity { get; set; }
    public uint Duration { get; set; }
}

public class GenericIn
{
    public string Id { get; set; }
    public string Name { get; set; }
}