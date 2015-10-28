using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TouchScript;
using System.IO.Ports;

public class ftlRobotManager : MonoBehaviour 
{
	public static Dictionary<int, ITouch> ftlTouches = new Dictionary<int, ITouch>();
	public static List<Robot> robots = new List<Robot>();
	public static Robot definedBot;
	public static string ComPort = "COM5";
	public static List<string> comPorts = new List<string> ();
	public static bool firstComPortSelected = false;


	void Awake () 
	{
	}

	void Start()
	{

	}


}
