using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;
    
    public GameObject LogGo;
    public GameObject LogPrefab;

    public List<LogItem> logItems = new();

    private void Awake()
    {
        Instance = this;
    }

    public void AddLog(GenericIni sender, ControlLog log)
    {
        var go = Instantiate(LogPrefab, LogGo.transform);
        var logItem = go.GetComponent<LogItem>();
        if (logItem == null)
        {
            Debug.LogError("Log item is null");
            return;
        }
        logItem.Configure(sender, log);
        logItems.Add(logItem);
        
    }
}
