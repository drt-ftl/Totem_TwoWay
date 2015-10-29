using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript;

public class ftlRobotDefiner : ftlRobotManager 
{
	
	private bool calibrated = false;
	private bool inPosition = false;
	private ITouch large;
	private ITouch small;
	private string instructions;
	
	void OnEnable () 
	{
		ftlTouches.Clear ();
		instructions = "CALIBRATION MODE: \r\n" +
			"Place your robot on the screen \r\n" +
				"and Press the Space bar.";
        XBeeManager.xBeeLineIn += LineIn;

		if (TouchManager.Instance != null)
		{
			TouchManager.Instance.TouchesBegan += touchesBeganHandler;
			TouchManager.Instance.TouchesEnded += touchesEndedHandler;
			TouchManager.Instance.TouchesMoved += touchesMovedHandler;
			TouchManager.Instance.TouchesCancelled += touchesCancelledHandler;
		}
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
	
	void OnGUI()
	{
		if (Input.GetKey(KeyCode.Escape))
			Application.Quit();
		
		if (Input.GetKey(KeyCode.Space))
			inPosition = true;
		
		GUI.Label (new Rect (10, 700, 300, 300), instructions);
	}
	void Update () 
	{
	}
	
    void DataIn(byte[] bytes)
    {
    }

    void LineIn(byte[] bytes)
    {
    }

    private void updateTouch(ITouch _touch)
	{
		ftlTouches[_touch.Id] = _touch;
	}
	
	#region Event handlers
	
	private void touchesBeganHandler(object sender, TouchEventArgs e)
	{
		foreach (var touch in e.Touches)
		{
			if (!touch.Properties.ContainsKey("Angle")) return;
			if (calibrated) return;
			if (ftlTouches.ContainsKey(touch.Id)) return;
			ftlTouches.Add (touch.Id, touch);
		}
	}
	
	private void touchesMovedHandler(object sender, TouchEventArgs e)
	{
		foreach (var touch in e.Touches)
		{
			ITouch testTouch;
			if (!ftlTouches.TryGetValue(touch.Id, out testTouch)) return;
			updateTouch(touch);
			
			foreach (var _touch in ftlTouches)
			{
				if (_touch.Key != touch.Id && Convert.ToDouble(touch.Properties["Area"].ToString()) > Convert.ToDouble(_touch.Value.Properties["Area"]))
				{
					var area = (float) Convert.ToDouble(touch.Properties["Area"].ToString());
					var _area = (float) Convert.ToDouble(_touch.Value.Properties["Area"].ToString());
					if (!calibrated)
					{
						if (area > _area)
						{
							large = touch;
							small = _touch.Value;
						}
					}
				}
			}
			
			if (inPosition && !calibrated)
			{
				definedBot = new Robot(small, large, Time.realtimeSinceStartup);
				OnDisable();
				calibrated = true;
				Camera.main.GetComponent<ftlRobotGatherer>().enabled = true;
				//Camera.main.GetComponent<GUIManager>().enabled = true;
				var col = Camera.main.GetComponent<Camera>().backgroundColor;
				col.b = col.b * 0.5f;
				Camera.main.GetComponent<Camera>().backgroundColor = col;
			}
		}
	}
	
	private void touchesEndedHandler(object sender, TouchEventArgs e)
	{
		if (calibrated) return;
		foreach (var touch in e.Touches)
		{
			ITouch _touch;
			if (!ftlTouches.TryGetValue(touch.Id, out _touch)) return;
			ftlTouches.Remove(touch.Id);
		}
	}
	
	private void touchesCancelledHandler(object sender, TouchEventArgs e)
	{
		touchesEndedHandler(sender, e);
	}
	
	#endregion
	
}
