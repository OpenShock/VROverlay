using ShockLink.API.Models;

namespace ShockLink.VROverlay
{
    public interface ILogReceiver
    {
        public void LogReceive(GenericIni sender, ControlLog log);
    }
}