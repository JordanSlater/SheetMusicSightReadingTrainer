using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;

public class ConsoleDisplayer : MonoBehaviour
{
    private TextMeshProUGUI textMeshPro;

    // Start is called before the first frame update
    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        Debug.Log("Start");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        textMeshPro.text += "\n" + logString;
        textMeshPro.text += "\n" + type;
        textMeshPro.text += "\n" + stackTrace;
    }

}
