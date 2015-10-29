using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;

using System.Threading;

public class UnitySerialPort
{
    public static UnitySerialPort Instance;
    public static string ComPort = "COM5";
    public static List<byte> currentBytesIn = new List<byte>();

    #region Properties

    public SerialPort SerialPort;
    public enum LoopUpdateMethod { Threading }
    public LoopUpdateMethod UpdateMethod = LoopUpdateMethod.Threading;
    private Thread serialThread;
    private ArrayList baudRates = new ArrayList() { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 };
    private ArrayList comPorts = new ArrayList();
    public bool OpenPortOnStart = false;
    private string portStatus = "";
    public string PortStatus
    {
        get { return portStatus; }
        set { portStatus = value; }
    }
    public static int BaudRate = 57600;
    public int ReadTimeout = 10;
    public int WriteTimeout = 10;
    private bool isRunning = false;
    public bool IsRunning
    {
        get { return isRunning; }
        set { isRunning = value; }
    }

    private string rawData = "Ready";
    public string RawData
    {
        get { return rawData; }
        set { rawData = value; }
    }

    private string[] chunkData;
    public string[] ChunkData
    {
        get { return chunkData; }
        set { chunkData = value; }
    }

    public GameObject ComStatusText;
    public GameObject RawDataText;
    #endregion Properties

    #region Unity Frame Events


    void GameObjectSerialPort_DataRecievedEvent(string[] Data, string RawData)
    {

    }

    public UnitySerialPort(XBeeManager manager)
    {
        Instance = this;
        PopulateComPorts();
        if (OpenPortOnStart) { OpenSerialPort(); }
    }

    void Update()
    {
        if (SerialPort == null || SerialPort.IsOpen == false) { return; }
        try
        {
            if (RawDataText != null)
                RawDataText.GetComponent<GUIText>().text = RawData;
        }
        catch
        {
        }
    }

    public void OnApplicationQuit()
    {
        CloseSerialPort();
        Thread.Sleep(500);
        if (UpdateMethod == LoopUpdateMethod.Threading)
        {
            StopSerialThread();
        }
        Thread.Sleep(500);
    }

    #endregion Unity Frame Events

    #region Object Serial Port

    public void OpenSerialPort()
    {
        try
        {
            // Initialise the serial port
            SerialPort = new SerialPort(ComPort, BaudRate);

            SerialPort.ReadTimeout = ReadTimeout;

            SerialPort.WriteTimeout = WriteTimeout;
            SerialPort.ReadBufferSize = 8192;
            SerialPort.WriteBufferSize = 8192;

            // Open the serial port
            SerialPort.Open();

            // Update the gui if applicable
            if (Instance != null && Instance.ComStatusText != null)
            { Instance.ComStatusText.GetComponent<GUIText>().text = "ComStatus: Open"; }

            if (UpdateMethod == LoopUpdateMethod.Threading)
            {
                // If the thread does not exist then start it
                if (serialThread == null)
                { StartSerialThread(); }
            }

        }
        catch {}
    }

    public void CloseSerialPort()
    {
        try
        {
            SerialPort.Close();
            if (Instance.ComStatusText != null)
            { Instance.ComStatusText.GetComponent<GUIText>().text = "ComStatus: Closed"; }
        }
        catch {}
    }

    #endregion Object Serial Port

    #region Serial Thread

    public void StartSerialThread()
    {
        try
        {
            serialThread = new Thread(new ThreadStart(SerialThreadLoop));
            isRunning = true;
            serialThread.Start();
        }
        catch {}
    }

    private void SerialThreadLoop()
    {
        while (isRunning)
        { GenericSerialLoop(); }
    }

    public void StopSerialThread()
    {
        isRunning = false;
        Thread.Sleep(100);
        if (serialThread != null)
        {
            serialThread.Abort();
            Thread.Sleep(100);
            serialThread = null;
        }
        if (SerialPort != null)
        { SerialPort = null; }
        portStatus = "Ended Serial Loop Thread";
    }

    #endregion Serial Thread

    #region Static Functions

    public void SendSerialDataAsDelimitedByteChunk(byte[] byteArray)
    {
        try
        {
            if (SerialPort != null)
            {
                SerialPort.Write(byteArray, 0, byteArray.Length);
            }
        }
        catch
        {
            SerialPort.DiscardOutBuffer(); //**************TEST!!!!!
            SerialPort.DiscardInBuffer();
        }
    }

    public void SendSerialDataAsLine(string data)
    {
        try
        {
            if (SerialPort != null)
            {
                SerialPort.WriteLine(data);
            }
        }
        catch
        {
            SerialPort.DiscardOutBuffer(); //**************TEST!!!!!
            SerialPort.DiscardInBuffer();
        }
    }

    public void SendSerialData(string data)
    {
        if (SerialPort != null)
        { SerialPort.Write(data); }
    }

    #endregion Static Functions

    private void ParseSerialData(string[] data, string rawData)
    {
    }

    public void PopulateComPorts()
    {
        foreach (string cPort in System.IO.Ports.SerialPort.GetPortNames())
        {
            comPorts.Add(cPort);
        }
    }

    public string UpdateComPort()
    {
        if (SerialPort != null && SerialPort.IsOpen)
        { CloseSerialPort(); }
        int currentComPort = comPorts.IndexOf(ComPort);
        portStatus = "ComPort set to: " + ComPort.ToString();
        return ComPort;
    }

    public int UpdateBaudRate()
    {
         if (SerialPort != null && SerialPort.IsOpen)
        { CloseSerialPort(); }
        int currentBaudRate = baudRates.IndexOf(BaudRate);
        if (currentBaudRate + 1 <= baudRates.Count - 1)
        {
            BaudRate = (int)baudRates[currentBaudRate + 1];
        }
        else
        {
            BaudRate = (int)baudRates[0];
        }
        portStatus = "BaudRate set to: " + BaudRate.ToString();
        return BaudRate;
    }

    private void GenericSerialLoop()
    {
        try
        {
            if (SerialPort.IsOpen)
            {
                var newByte = (byte)SerialPort.ReadByte();
                var newByteArray = new byte[1];
                newByteArray[0] = newByte;
                XBeeManager.XBeeDataIn(newByteArray);
                
                if (currentBytesIn.Count < 6)
                //if (newByte != 124) // Pipe Character
                {
                    currentBytesIn.Add(newByte);
                }
                else
                {
                    var b = currentBytesIn.ToArray();
                    currentBytesIn.Clear();
                    XBeeManager.XBeeLineIn(b);
                }
            }
        }
        catch { }
    }

}

