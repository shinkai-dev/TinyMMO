

using Godot;
using System;

public class Player : KinematicBody2D
{
	[Export] public int MoveSpeed = 16;
	public Sprite sprite;
	[Puppet] public Vector2 PuppetVelocity = new Vector2();
	[Puppet] public Vector2 PuppetPosition = new Vector2();
	public Vector2 Velocity = new Vector2();

	public override void _Ready()
	{
		sprite = GetNode<Sprite>("Playa");
		PuppetPosition = Position;
		PuppetVelocity = Velocity;
	}
	
	public void GetInput(float delta)
	{
		Velocity = new Vector2();

		if (Input.IsActionPressed("ui_right"))
			Velocity.x += 1;

		if (Input.IsActionPressed("ui_left"))
			Velocity.x -= 1;

		if (Input.IsActionPressed("ui_down"))
			Velocity.y += 1;

		if (Input.IsActionPressed("ui_up"))
			Velocity.y -= 1;

		Velocity *= delta * MoveSpeed;
		Rset(nameof(PuppetVelocity), Velocity);
		Rset(nameof(PuppetPosition), Position);
	}
	
	public override void _PhysicsProcess(float delta)
	{
		if (IsNetworkMaster())
		{
			GetInput(delta);
		} else {
			Position = PuppetPosition;
			Velocity = PuppetVelocity;
		}

		sprite.FlipH = Velocity.x < 0;

		MoveAndCollide(Velocity);
	}
}

