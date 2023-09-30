using System.Collections.Generic;

namespace ShockLink.API.Models
{
    public class DeviceWithShockers : Device
    {
        public IEnumerable<ShockerResponse> Shockers { get; set; }
    }
}