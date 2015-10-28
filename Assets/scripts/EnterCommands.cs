using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TouchScript;

public class EnterCommands : ftlRobotManager 
{
	public enum DataTypeToSend {Position, Velocity, Angle, Speed, SpeedSquared};
	public List<DataTypeToSend> dataTypeToSend = new List<DataTypeToSend>();

	// Use this for initialization
	void OnEnable () 
	{
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

	void Update () 
	{
	
	}

	private void updateTouch(ITouch _touch)
	{

	}

	#region Event handlers
	
	private void touchesBeganHandler(object sender, TouchEventArgs e)
	{
		foreach (var touch in e.Touches)
		{
			if (!touch.Properties.ContainsKey("Angle")) return;	
			if (touch.Hit != null)
			{
			}
		}
	}
	
	private void touchesMovedHandler(object sender, TouchEventArgs e)
	{
		foreach (var touch in e.Touches)
		{
			if (!touch.Properties.ContainsKey("Angle")) return;
			ITouch testTouch;
			updateTouch(touch);
		}
	}
	
	private void touchesEndedHandler(object sender, TouchEventArgs e)
	{
		foreach (var touch in e.Touches)
		{
			ITouch _touch;
			if (!ftlTouches.TryGetValue(touch.Id, out _touch)) return;
		}
	}
	
	private void touchesCancelledHandler(object sender, TouchEventArgs e)
	{
		touchesEndedHandler(sender, e);
	}
	
	#endregion
}
