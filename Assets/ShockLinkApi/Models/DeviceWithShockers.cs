using System.Collections.Generic;

public class DeviceWithShockers : Device
{
    public IEnumerable<ShockerResponse> Shockers { get; set; }
}