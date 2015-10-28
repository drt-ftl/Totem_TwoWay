using UnityEngine;
using System;
using System.Collections;
using TouchScript;

public class RobotMarker
{
	private ITouch touch;
	private bool isHead;

	public RobotMarker (ITouch _touch, bool _head)
	{
		Touch = _touch;
		IsHead = _head;
	}
	public ITouch Touch
	{
		get { return touch;}
		set { touch = value;}
	}
	
	public float Angle
	{
		get 
		{
			var angle = (float)Convert.ToDouble(Touch.Properties["Angle"].ToString());
			return angle;
		}
	}

	public bool IsHead
	{
		get { return isHead;}
		set { isHead = false;}
	}
}
