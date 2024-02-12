using Godot;
using System;

public partial class PlayerCam : Camera2D
{
	[Export] public float ServerCamSpeed = 32f;

	public override void _Process(double delta)
	{
		if (Multiplayer.GetUniqueId() == 1) {
			UpdateServerCam(delta);
		} else {
			UpdatePlayerCam();
		}
	}

	void UpdateServerCam(double delta) {
		var Velocity = new Vector2();

		if (Input.IsActionPressed("ui_right"))
			Velocity.X += 1;

		if (Input.IsActionPressed("ui_left"))
			Velocity.X -= 1;

		if (Input.IsActionPressed("ui_down"))
			Velocity.Y += 1;

		if (Input.IsActionPressed("ui_up"))
			Velocity.Y -= 1;
		
		Velocity = Velocity * (float)delta * ServerCamSpeed;

		GlobalPosition += Velocity;
	}

	void UpdatePlayerCam() {
		try {
		 	var PlayerPos = GetNode<Player>("../" + Multiplayer.GetUniqueId()).GlobalTransform;
			GlobalPosition = PlayerPos.Origin;
		} catch {
			// Do nothing
		}
	}
}
