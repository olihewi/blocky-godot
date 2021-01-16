using Godot;
using System;

public class Orbit : Spatial
{
	[Export]
	public float cameraMoveSpeed = 1;

	[Export]
	public float cameraSensitivity = 1;

	private Spatial rotationHelper;
	private Spatial camera;
	private float mouseX = 0;
	private float mouseY = 0;

	private Vector3 moveVector;
	private Vector3 _dir;

	private bool mousePressed = true;
	
	public override void _Ready()
	{
		rotationHelper = GetNode<Spatial>("Rotation Helper");
		camera = GetNode<Spatial>("Rotation Helper/Camera");
		Input.SetMouseMode(Input.MouseMode.Captured);
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
public override void _Input(InputEvent @event)
{
	if (@event is InputEventMouseMotion mouseEvent && mousePressed)
	{
		rotationHelper.RotateX(Mathf.Deg2Rad(-mouseEvent.Relative.y * cameraSensitivity));
		RotateY(Mathf.Deg2Rad(-mouseEvent.Relative.x * cameraSensitivity));
	}
	
	if (@event is InputEventMouseButton clickEvent && (ButtonList)clickEvent.ButtonIndex == ButtonList.Left)
	{
		if (clickEvent.Pressed)
		{
			Input.SetMouseMode(Input.MouseMode.Captured);
		}
	}
}

public override void _Process(float delta)
	{
		moveVector = new Vector3();
		Transform aim = camera.GlobalTransform;
		if (Input.IsActionPressed("Forward"))
		{
			moveVector.z -= 1;
		}
		if (Input.IsActionPressed("Backwards"))
		{
			moveVector.z += 1;
		}
		if (Input.IsActionPressed("Left"))
		{
			moveVector.x -= 1;
		}
		if (Input.IsActionPressed("Right"))
		{
			moveVector.x += 1;
		}
		if (Input.IsActionPressed("Up"))
		{
			moveVector.y += 1;
		}
		if (Input.IsActionPressed("Down"))
		{
			moveVector.y -= 1;
		}

		if (Input.IsActionPressed("Sprint"))
		{
			moveVector *= 2;
		}

		Translate(moveVector * cameraMoveSpeed * delta);
	}
}
