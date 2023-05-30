using System;
using BestHTTP.SignalRCore;

public sealed class ShockLinkAuthenticator : IAuthenticationProvider
{
    // No pre-auth step required for this type of authentication
    public bool IsPreAuthRequired => false;

    // Not used as IsPreAuthRequired is false
    public event OnAuthenticationSuccededDelegate OnAuthenticationSucceded;

    // Not used as IsPreAuthRequired is false
    public event OnAuthenticationFailedDelegate OnAuthenticationFailed;

    private readonly string value;

    public ShockLinkAuthenticator(string value)
    {
        this.value = value;
    }
    
    // Not used as IsPreAuthRequired is false
    public void StartAuthentication()
    {
    }

    public void PrepareRequest(BestHTTP.HTTPRequest request) => request.SetHeader("ShockLinkToken", value);

    public Uri PrepareUri(Uri uri) => uri;

    public void Cancel()
    {
    }
}