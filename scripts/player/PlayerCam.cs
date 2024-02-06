using Godot;
using System;

public partial class PlayerCam : Camera2D
{
	[Export] public float ServerCamSpeed = 32f;

	public override void _Process(float delta)
	{
		if (GetTree().GetUniqueId() == 1) {
			UpdateServerCam(delta);
		} else {
			UpdatePlayerCam();
		}
	}

	void UpdateServerCam(float delta) {
		var Velocity = new Vector2();

		if (Input.IsActionPressed("ui_right"))
			Velocity.x += 1;

		if (Input.IsActionPressed("ui_left"))
			Velocity.x -= 1;

		if (Input.IsActionPressed("ui_down"))
			Velocity.y += 1;

		if (Input.IsActionPressed("ui_up"))
			Velocity.y -= 1;
		
		Velocity *= delta * ServerCamSpeed;

		GlobalPosition += Velocity;
	}

	void UpdatePlayerCam() {
		try {
		 	var PlayerPos = GetNode<Player>("../" + GetTree().GetUniqueId()).GlobalTransform;
			GlobalPosition = PlayerPos.origin;
		} catch {
			// Do nothing
		}
	}
}
