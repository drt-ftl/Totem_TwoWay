using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript;
using System.Threading;
using System.Timers;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class ftlRobotGatherer : ftlRobotManager 
{
	private bool calibrated = false;
	public LineRenderer lineRnd;
	private float headArea;
	private Vector2 headSize;
	private float headToTailDistance;
	private float tailArea;
	private Vector2 tailSize;
	private ITouch tmpHead;
	private ITouch tmpTail;
	private string instructions;
	public Material mat;
	public Vector2 minMaxAngles;
	private System.Timers.Timer _timer1;
	private Vector2 worldToScreenRatio = new Vector2();
	private Vector2 pos = new Vector2();
	private Vector2 vel = new Vector2();
    private int ind;
	private float ang = 0;
    public GUISkin skin;
    private string totemPosX = "";
    private string totemPosY = "";
    private string totemFmag = "";
    private string totemFangle = "";
    [DllImport("user32.dll")]
    private static extern void ShowDialog();
    [DllImport("user32.dll")]
    private static extern void SaveFileDialog();
    public List<TotemInputPacket> inputPackets = new List<TotemInputPacket>();
    bool show = true;


    void OnEnable () 
	{
		XBeeManager.xBeeLineIn += LineIn;
		var max = Camera.main.ScreenToWorldPoint (new Vector3 (UnityEngine.Screen.width, UnityEngine.Screen.height, 0));
		worldToScreenRatio = new Vector2 ((max.x * 2) / UnityEngine.Screen.width, (max.y * 2) / UnityEngine.Screen.height);
		Thread.Sleep (10);
		Camera.main.GetComponent<ftlRobotDefiner> ().enabled = false;
		Thread.Sleep (10);
		ftlTouches.Clear ();
		
		headArea = (float)Convert.ToDouble(definedBot.Head.Touch.Properties["Area"].ToString ());
		var headW = (float)Convert.ToDouble(definedBot.Head.Touch.Properties["Width"].ToString ());
		var headH = (float)Convert.ToDouble(definedBot.Head.Touch.Properties["Height"].ToString ());
		headSize = new Vector2 (headW, headH);
		
		tailArea = (float)Convert.ToDouble(definedBot.Tail.Touch.Properties["Area"].ToString ());
		var tailW = (float)Convert.ToDouble(definedBot.Tail.Touch.Properties["Width"].ToString ());
		var tailH = (float)Convert.ToDouble(definedBot.Tail.Touch.Properties["Height"].ToString ());
		tailSize = new Vector2 (tailW, tailH);
		
		headToTailDistance = Vector2.Distance (definedBot.Head.Touch.Position, definedBot.Tail.Touch.Position);
		minMaxAngles = new Vector2 (180f, 180f);
		
		if (TouchManager.Instance != null)
		{
			TouchManager.Instance.TouchesBegan += touchesBeganHandler;
			TouchManager.Instance.TouchesEnded += touchesEndedHandler;
			TouchManager.Instance.TouchesMoved += touchesMovedHandler;
			TouchManager.Instance.TouchesCancelled += touchesCancelledHandler;
		}
		
		instructions = "TRACKING MODE: \r\n" +
			"Red: Large Marker \r\n" +
				"Blue: Small Marker \r\n" +
				"Yellow: Centerline";
		
		_timer1 = new System.Timers.Timer (1000);
		_timer1.Elapsed += new System.Timers.ElapsedEventHandler (unity_Tick);
		_timer1.Enabled = true;
		_timer1.Start ();
		
	}
	
	private void OnDisable()
	{
		if (TouchManager.Instance != null)
		{
			TouchManager.Instance.TouchesBegan -= touchesBeganHandler;
			TouchManager.Instance.TouchesEnded -= touchesEndedHandler;
			TouchManager.Instance.TouchesMoved -= touchesMovedHandler;
			TouchManager.Instance.TouchesCancelled -= touchesCancelledHandler;
		}
	}
	
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            show = false;
            XBeeManager.unitySerialPort.OnApplicationQuit();
            UnityEngine.Application.Quit();
        }
    }
	void OnGUI()
	{
        if (!show) return;
        var rect = new Rect(5, 5, 250, 500);
        GUI.skin = skin;
        GUI.Window(0, rect, guiDisplay, "Inspector");
        foreach (var robo in robots)
		{
			var dist = Vector2.Distance(robo.Head.Touch.Position, robo.Tail.Touch.Position);
			if (dist >= headToTailDistance * 0.9f && dist <= headToTailDistance * 1.1f)
			{
                #region GL
                GL.Begin (GL.LINES);
				mat.SetPass (0);
				GL.Color (Color.yellow);
				GL.Vertex (new Vector3( robo.Head.Touch.Position.x ,UnityEngine.Screen.height - robo.Head.Touch.Position.y , 0));
				GL.Vertex (new Vector3( robo.Tail.Touch.Position.x ,UnityEngine.Screen.height - robo.Tail.Touch.Position.y , 0));
				GL.End ();
				
				var centerLine = robo.Head.Touch.Position - robo.Tail.Touch.Position;
				centerLine.y = -centerLine.y;
				var orthoHeadLine = new Vector2 (centerLine.y * 0.2f, centerLine.x * 0.2f);
				var headLine1 = robo.Head.Touch.Position - orthoHeadLine;
				var headLine2 = robo.Head.Touch.Position + orthoHeadLine;
				GL.Begin (GL.LINES);
				mat.SetPass (0);
				GL.Color (Color.red);
				GL.Vertex (new Vector3( robo.Head.Touch.Position.x ,UnityEngine.Screen.height - robo.Head.Touch.Position.y , 0));
				GL.Vertex (new Vector3( headLine1.x ,UnityEngine.Screen.height - headLine1.y , 0));
				GL.End ();
				GL.Begin (GL.LINES);
				GL.Color (Color.red);
				GL.Vertex (new Vector3( robo.Head.Touch.Position.x ,UnityEngine.Screen.height - robo.Head.Touch.Position.y , 0));
				GL.Vertex (new Vector3( headLine2.x ,UnityEngine.Screen.height - headLine2.y , 0));
				GL.End ();
				
				var orthoTailLine = new Vector2 (centerLine.y * 0.3f, centerLine.x * 0.3f);
				var tailLine1 = robo.Tail.Touch.Position - orthoTailLine;
				var tailLine2 = robo.Tail.Touch.Position + orthoTailLine;
				
				GL.Begin (GL.LINES);
				GL.Color (Color.blue);
				GL.Vertex (new Vector3( robo.Tail.Touch.Position.x ,UnityEngine.Screen.height - robo.Tail.Touch.Position.y , 0));
				GL.Vertex (new Vector3( tailLine1.x ,UnityEngine.Screen.height - tailLine1.y , 0));
				GL.End ();
				
				GL.Begin (GL.LINES);
				GL.Color (Color.blue);
				GL.Vertex (new Vector3( robo.Tail.Touch.Position.x ,UnityEngine.Screen.height - robo.Tail.Touch.Position.y , 0));
				GL.Vertex (new Vector3( tailLine2.x ,UnityEngine.Screen.height - tailLine2.y , 0));
				GL.End ();

                if (robo.Centroid.x > 422 - 40 && robo.Centroid.x < 422 + 40)
                {
                    GL.Begin(GL.LINES);
                    GL.Color(Color.yellow);
                    GL.Vertex(new Vector3(422, UnityEngine.Screen.height, 0));
                    GL.Vertex(new Vector3(422, 0, 0));
                    GL.End();
                }

                if (robo.Centroid.y > 476 - 40 && robo.Centroid.y < 476 + 40)
                {
                    GL.Begin(GL.LINES);
                    GL.Color(Color.yellow);
                    GL.Vertex(new Vector3(422, UnityEngine.Screen.height - 476, 0));
                    GL.Vertex(new Vector3(UnityEngine.Screen.width, UnityEngine.Screen.height - 476, 0));
                    GL.End();
                }

                if (robo.Centroid.x > 844 - 40 && robo.Centroid.x < 844 + 40)
                {
                    GL.Begin(GL.LINES);
                    GL.Color(Color.yellow);
                    GL.Vertex(new Vector3(844, UnityEngine.Screen.height, 0));
                    GL.Vertex(new Vector3(844, 0, 0));
                    GL.End();
                }

                if (robo.Centroid.y > 198 - 40 && robo.Centroid.y < 198 + 40)
                {
                    GL.Begin(GL.LINES);
                    GL.Color(Color.yellow);
                    GL.Vertex(new Vector3(844, UnityEngine.Screen.height - 198, 0));
                    GL.Vertex(new Vector3(UnityEngine.Screen.width, UnityEngine.Screen.height - 198, 0));
                    GL.End();
                }
                #endregion

                var a = robo.Angle.ToString("f0");
				if (robo.Angle < minMaxAngles.x)
					minMaxAngles.x = robo.Angle;
				if (robo.Angle > minMaxAngles.y)
					minMaxAngles.y = robo.Angle;
				ind = robots.IndexOf(robo);
				pos = robo.Centroid;
				vel = robo.Velocity;
				ang = robo.Angle;
			}
		}
	}
	
    private void guiDisplay (int _i)
    {
        var rect = new Rect(10, 30, 220, 490);
        GUILayout.BeginArea(rect);
        GUILayout.Box("Robot[" + ind.ToString() + "]");
        GUILayout.Box("Angle: " + ang.ToString("f0"));
        GUILayout.Box("Centroid: " + pos.ToString("f0"));
        GUILayout.Box("Robot: " + vel.ToString("f2"));
        if (GUILayout.Button("Save"))
        {
            saveFile();
        }
        GUILayout.Label(instructions);
        GUILayout.EndArea();
    }

    public void saveFile()
    {
        System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
        saveFileDialog.InitialDirectory = UnityEngine.Application.dataPath + "/Samples";
        var sel = "CSV Files (*.csv)|*.csv";
        saveFileDialog.Filter = sel;
        saveFileDialog.RestoreDirectory = true;
        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var tw = File.CreateText(saveFileDialog.FileName);
            tw.WriteLine("Force Magnitude, Force Angle, Position(X), Position(Y)");
            foreach (var ip in inputPackets)
                tw.WriteLine(ip.TotemString); 
            tw.Close();
            inputPackets.Clear();
        }
    }

    private void updateTouch(ITouch _touch)
	{
		if (ftlTouches.ContainsKey(_touch.Id))
			ftlTouches[_touch.Id] = _touch;
		if (robots.Count >= 1)
			robots [0].Add (Time.realtimeSinceStartup);
	}

    #region Event handlers

    unsafe static byte[] convertBytes (Vector2 _pos, Vector2 _vel, float _ang)
    {
        var x = (int)_pos.x;
        byte* xSplit = (byte*)&x;
        var y = (int)_pos.y;
        byte* ySplit = (byte*)&y;
        var xv = (int)(_vel.x * 100); // Multiply velocity and angle by 100 to get 2 decimal places
        byte* xvSplit = (byte*)&xv;
        var yv = (int)(_vel.y * 100);
        byte* yvSplit = (byte*)&yv;
        var a = (int)(_ang * 100);
        byte* aSplit = (byte*)&a;

        byte[] bytes = new byte[12];      
        bytes[0] = xSplit[0];
        bytes[1] = xSplit[1];
        bytes[2] = ySplit[0];
        bytes[3] = ySplit[1];
        bytes[4] = xvSplit[0];
        bytes[5] = xvSplit[1];
        bytes[6] = xvSplit[2];
        bytes[7] = yvSplit[0];
        bytes[8] = yvSplit[1];
        bytes[9] = yvSplit[2];
        bytes[10] = aSplit[0];
        bytes[11] = aSplit[1];
        return bytes;
    }
    void LineIn(byte[] bytes)
    {
        XBeeManager.unitySerialPort.SerialPort.DiscardInBuffer();
        var line = System.Text.Encoding.ASCII.GetString(bytes);
        print(line);
        //var chunks = line.Split(',');
        var newTotemPacket = new TotemInputPacket();
        newTotemPacket.TotemString = line;
        inputPackets.Add(newTotemPacket);

        if (XBeeManager.unitySerialPort.SerialPort.IsOpen && robots.Count >= 1)
        {
            //XBeeManager.unitySerialPort.SendSerialDataAsLine("0,"
            //                                                 + ang.ToString("f2") + ","
            //                                                 + ((int)pos.x).ToString() + ","
            //                                                 + ((int)pos.y).ToString() + ","
            //                                                 + vel.x.ToString("f2") + ","
            //                                                 + vel.y.ToString("f2"));
            XBeeManager.unitySerialPort.SendSerialDataAsDelimitedByteChunk(convertBytes(pos, vel, ang));
        }
    }

    private void touchesBeganHandler(object sender, TouchEventArgs e)
	{
		foreach (var touch in e.Touches)
		{
			if (ftlTouches.ContainsKey(touch.Id)) return;
			if (!touch.Properties.ContainsKey("Angle")) return;
			ftlTouches.Add (touch.Id, touch);
		}
	}
	
	private void touchesMovedHandler(object sender, TouchEventArgs e)
	{
		foreach (var touch in e.Touches)
		{
			ITouch testTouch;
			if (!ftlTouches.TryGetValue(touch.Id, out testTouch))
			{
				ftlTouches.Add (touch.Id, touch);
			}
			updateTouch(touch);
			foreach (var r in robots)
			{
				if (r.Head.Touch.Id == touch.Id || r.Tail.Touch.Id == touch.Id)
				{
					return;
				}
			}
			var area = (float)Convert.ToDouble(touch.Properties["Area"].ToString ());
			var w = (float)Convert.ToDouble(touch.Properties["Width"].ToString ());
			var h = (float)Convert.ToDouble(touch.Properties["Height"].ToString ());
			if (area >= headArea * 0.9f && area <= headArea * 1.1f)
			{
				tmpHead = touch;
				if (tmpTail != null)
				{
					var robo = new Robot(touch, tmpTail, Time.realtimeSinceStartup);
					robots.Add (robo);
					tmpHead = null;
					tmpTail = null;
				}
			}
			if (area >= tailArea * 0.9f && area <= tailArea * 1.1f)
			{
				tmpTail = touch;
				if (tmpHead != null)
				{
					var robo = new Robot(tmpHead, touch, Time.realtimeSinceStartup);
					robots.Add (robo);
					tmpHead = null;
					tmpTail = null;
				}
			}
		}
	}
	
	private void touchesEndedHandler(object sender, TouchEventArgs e)
	{
		foreach (var touch in e.Touches)
		{
			ITouch _touch;
			if (!ftlTouches.TryGetValue(touch.Id, out _touch)) return;
			if (tmpHead == touch || tmpTail == touch)
			{
				tmpHead = null;
				tmpTail = null;
			}
			List<int> robotsToDestroy = new List<int>();
			foreach (var robo in robots)
			{
				if (robo.Head.Touch.Id == touch.Id || robo.Tail.Touch.Id == touch.Id)
				{
					robotsToDestroy.Add (robots.IndexOf(robo));
				}
			}
			foreach (var d in robotsToDestroy)
			{
				robots.RemoveAt(d);
			}
			ftlTouches.Remove(touch.Id);
		}
	}
	
	private void touchesCancelledHandler(object sender, TouchEventArgs e)
	{
		touchesEndedHandler(sender, e);
	}
	
	#endregion
	
	private void unity_Tick(object sender,  System.Timers.ElapsedEventArgs e)
    { 
    }
}
