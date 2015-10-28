using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TouchScript;
using System;

public class Robot
{
	private List <RobotFrame> frames;
	private RobotMarker head; // headTouch will be the larger tag
	private RobotMarker tail; // tailTouch will be the smaller tag
	private float desiredAngle;
	private Vector2 desiredCentroid;
	private LineRenderer lineRnd;
	private int cycles = 0;


	public Robot (ITouch touchHead, ITouch touchTail, float currentTime)
	{
		Head = new RobotMarker (touchHead, false);
		Tail = new RobotMarker (touchTail, false);
		frames = new List<RobotFrame> ();
	}

	public RobotMarker Head
	{
		get { return head;}
		set { head = value;}
	}

	public RobotMarker Tail
	{
		get { return tail;}
		set { tail = value;}
	}

	public void Add (float currentTime)
	{
		var newFrame = new RobotFrame (RawCentroid, RawAngle, currentTime);
		if (frames.Count >= 5)
			frames.RemoveAt (0);
		frames.Add (newFrame);
	}
	
	public float RawAngle
	{
		get 
		{
			var diff = Head.Touch.Position - Tail.Touch.Position;
			var angle = -Mathf.Atan2 (diff.y, diff.x) * Mathf.Rad2Deg + 180f;
			if (frames.Count < 2)
			{
				cycles = 0;
				return angle;
			}
			else
			{
				var lastAngle = frames[frames.Count - 2].Angle;
				if (Mathf.Abs (angle - lastAngle) > 90) { // if there is a significant change
					if (lastAngle < angle) {  // ex. jumps from 12 to 160 -- Counterclockwise
						cycles--;
					} else { // ex. jumps from 175 to 4 -- Clockwise
						cycles++;
					}
				}
				//angle += cycles * 360f;
			}
			return angle;
		}
	}

	public float Angle
	{
		get{
			if (frames.Count < 1)
				return 0;
			var sum = 0f;
			foreach (var a in frames)
			{
				sum += a.Angle;
			}
			var average = sum / frames.Count;
			average -= 90;
			if (average < 0)
				average += 360;
			average = 360 - average;
			return average;
		}
	}

	public Vector2 RawCentroid
	{
		get
		{
			var rawCentroid = new Vector2 ();
			rawCentroid.x = (Head.Touch.Position.x + Tail.Touch.Position.x) / 2;
			rawCentroid.y = (Head.Touch.Position.y + Tail.Touch.Position.y) / 2;
			//rawCentroid = (Vector2)Camera.main.ScreenToWorldPoint(rawCentroid);
			return rawCentroid;
		}
	}

	public Vector2 Centroid
	{
		get
		{
			if (frames.Count < 1)
				return Vector2.zero;
			var centroid = new Vector2();
			foreach (var f in frames)
			{
				centroid += f.Centroid;
			}
			centroid = centroid / frames.Count;
			return centroid;
		}
	}

	public float DesiredAngle
	{
		get { return desiredAngle;}
		set { desiredAngle = value;}
	}

	public Vector2 DesiredCentroid
	{
		get	{ return desiredCentroid;}
		set	{ desiredCentroid = value;}
	}

	public LineRenderer LineRnd
	{
		get	
		{ 
			lineRnd.SetVertexCount(2);
			var headPos = Camera.main.ScreenToWorldPoint(Head.Touch.Position);
			headPos.z = 0;
			var tailPos = Camera.main.ScreenToWorldPoint(Tail.Touch.Position);
			tailPos.z = 0;
			lineRnd.SetPosition(0, headPos);
			lineRnd.SetPosition(1, headPos);
			return lineRnd;
		}
		set	{ lineRnd = value;}
	}

	public Vector2 Velocity
	{
		get 
		{
			if (frames.Count < 2)
				return new Vector2(5,4);
			var velocity = new Vector2();
			var topIndex = frames.Count - 1;
			for (int i = 1; i <= topIndex; i++)
			{
				var f = frames[i];
				var _f = frames[i-1];
				velocity += ((f.Centroid - _f.Centroid) / (f.FrameTime - _f.FrameTime));
			}
			velocity = velocity / frames.Count;
			return velocity;
		}
	}

	public float Speed
	{
		get{ return 0;}
	}

	public float SpeedSquared
	{
		get{ return 0;}
	}
}
