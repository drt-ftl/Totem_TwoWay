using UnityEngine;
using System.Collections;

public class RobotFrame
{
	public RobotFrame (Vector2 centroid, float angle, float frameTime)
	{
		Centroid = centroid;
		Angle = angle;
		FrameTime = frameTime;
	}

	public Vector2 Centroid { get; set; }

	public float Angle { get; set; }

	public float FrameTime { get; set; }
}
