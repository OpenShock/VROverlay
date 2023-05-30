using System;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using UnityEngine;

public static class ShockLinkUserHub
{
    private static readonly HubConnection Connection = new(
        new Uri("https://api.shocklink.net/1/hubs/user"), new JsonProtocol(new LitJsonEncoder()), new HubOptions
        {
            PreferedTransport = TransportTypes.WebSocket
        });
    
    static ShockLinkUserHub()
    {
        Connection.ReconnectPolicy = new DefaultRetryPolicy();
        Connection.AuthenticationProvider =
            new ShockLinkAuthenticator("9FLJsjfDOD8y2pKQvSQL0qYJxjYcCXnqwtJ3QFPOiCwulhEDFQFzoen3nVeJo5ry");
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
