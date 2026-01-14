using Godot;
using System;

public partial class PlayerController : Node3D
{
	[Export]
	public Camera3D camera;

	private float cameraDesiredAltitude = 5f;
	private float cameraFollowSpeed = 5.0f;

	private float sensitivity = 0.050f;

	private float cameraAngle = 0f;

	public Node3D anchor;
  [Export]
  public Node3D shitpoint;
	private float desiredAltitude = 1.5f; // Desired distance from Origin.

	public override void _Ready()
	{
		anchor = (Node3D)GetChild(0);
		anchor.Position = new Vector3(0f, desiredAltitude, 0f);

	}

	public override void _Process(double delta)
	{

		// Movement
		Basis visualsBasis = anchor.Transform.Basis;

		anchor.Position = visualsBasis.Y.Normalized() * desiredAltitude;



		Vector2 movementVector = Input.GetVector("move_right","move_left","move_down","move_up");
	shitpoint.Position = new Vector3(movementVector.X,0f,movementVector.Y);
	Vector3 globalMovementVector = (shitpoint.GlobalPosition - anchor.GlobalPosition).Cross(visualsBasis.Y).Normalized();

	  if (movementVector.LengthSquared() > 0)
  		anchor.RotateObjectLocal(globalMovementVector, (float)delta);
	 
  }

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			if (Input.IsActionPressed("rotate_view"))
			{
				float degree = eventMouseMotion.Velocity.X;
				cameraAngle += degree;
		GD.Print(Mathf.DegToRad(degree * GetProcessDeltaTime()));
			}
		}
	}

}
