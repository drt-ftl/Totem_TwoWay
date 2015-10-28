using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Text;

public class XBeeManager : MonoBehaviour
{
    public static XBeeManager Instance;
    public static UnitySerialPort unitySerialPort;
    public static List<string> comPorts = new List<string>();
    public static bool firstComPortSelected = false;
    public static GUISkin GUISkin;
    public static GUIStyle guiStyle = new GUIStyle();
    public static string OutputString;

    private string PortOpenStatus;
    private bool showList = false;
    public int listEntry = 0;
    public Popup.ListCallBack callback;
    public static int SendBytesPerPacket = 10;
    public static int ReceiveBytesPerPacket = 14;
    public int _baudRate = 57600;
    public ArrayList baudRates = new ArrayList() { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 };


    void Awake()
    {
        UnitySerialPort.BaudRate = _baudRate;
        Instance = this;
        foreach (var availablePort in SerialPort.GetPortNames())
        {
            comPorts.Add(availablePort);
        }
        if (comPorts.Count >= 1)
        {
            UnitySerialPort.ComPort = comPorts[0];
        }
        callback = new Popup.ListCallBack(PrintResults);
        guiStyle.alignment = TextAnchor.MiddleCenter;
        guiStyle.normal.textColor = new Color(1f, 1f, 1f, 1f);
        guiStyle.focused.textColor = Color.red;
        var tex = new Texture2D(2, 2);
        var colors = new Color[4];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        guiStyle.hover.background = tex;
        guiStyle.onHover.background = tex;
        guiStyle.padding.left = guiStyle.padding.right = guiStyle.padding.top = guiStyle.padding.bottom = 4;
        guiStyle.onActive.background = tex;
    }

    void Start()
    {
        unitySerialPort = new UnitySerialPort(Instance);
        unitySerialPort = UnitySerialPort.Instance;
    }

    void Update()
    {
        if (unitySerialPort.SerialPort == null)
        {
            PortOpenStatus = "Open Port"; return;
        }
        switch (unitySerialPort.SerialPort.IsOpen)
        {
            case true: PortOpenStatus = "Close Port"; break;
            case false: PortOpenStatus = "Open Port"; break;
        }
    }

    void OnGUI()
    {
        if (unitySerialPort.SerialPort != null) return;
        if (GUISkin != null) { GUI.skin = GUISkin; }
        GUILayout.BeginArea(new Rect(Screen.width - 205, 10, 200, 200), "", GUI.skin.box);
        var content = new GUIContent();
        if (Popup.List(new Rect(5, 5, 180, 100), ref showList, ref listEntry, content, comPorts, guiStyle, callback, ref firstComPortSelected))
        {
            UnitySerialPort.ComPort = comPorts[listEntry];
        }
        GUILayout.Space(140);
        if (GUILayout.Button(PortOpenStatus, GUILayout.Height(30)))
        {
            if (unitySerialPort.SerialPort == null)
            { unitySerialPort.OpenSerialPort(); return; }

            switch (unitySerialPort.SerialPort.IsOpen)
            {
                case true: unitySerialPort.CloseSerialPort(); break;
                case false: unitySerialPort.OpenSerialPort(); break;
            }
        }
        GUILayout.EndArea();
    }

    public void PrintResults()
    {
    }

    void OnApplicationQuit()
    {
        unitySerialPort.OnApplicationQuit();
    }

    public delegate void XBeeEventHandler(byte[] _bytes);
    public static event XBeeEventHandler xBeeDataIn;
    public static event XBeeEventHandler xBeeLineIn;

    public static void XBeeDataIn(byte[] _bytes)
    {
        if (xBeeDataIn != null && _bytes != null && _bytes.Length > 0)
            xBeeDataIn(_bytes);
    }

    public static void XBeeLineIn(byte[] _bytes)  // Gets Triggered when two consecutive 4F bytes (int 79) are detected.
    {                                           // Since max data resolution will be 14 bytes, two 4F bytes should never be seen
        if (xBeeLineIn != null && _bytes != null && _bytes.Length > 0)
            xBeeLineIn(_bytes);
    }
}

