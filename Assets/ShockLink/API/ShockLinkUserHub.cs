using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using ShockLink.API;
using ShockLink.API.Models;
using UnityEngine;

namespace ShockLink.VROverlay
{
    public static class ShockLinkUserHub
    {
        private static readonly HubConnection Connection = new(
            Config.ConfigInstance.ShockLink.UserHub, new JsonProtocol(new LitJsonEncoder()), new HubOptions
            {
                PreferedTransport = TransportTypes.WebSocket
            });

        static ShockLinkUserHub()
        {
            Connection.ReconnectPolicy = new DefaultRetryPolicy();
            Connection.AuthenticationProvider =
                new ShockLinkAuthenticator(Config.ConfigInstance.ShockLink.ApiToken);
            Connection.On<GenericIni, ControlLog[]>("Log", (sender, logs) =>
            {
                foreach (var log in logs) UiManager.Instance.AddLog(sender, log);
            });
        }

        public static void Start()
        {
            if (Connection.State is not (ConnectionStates.Initial or ConnectionStates.Closed)) return;
            Debug.Log("Starting SignalR Connection");
            Connection.StartConnect();
        }
    }
}