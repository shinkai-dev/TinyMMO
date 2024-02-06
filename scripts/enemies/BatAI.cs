using Godot;
using System;

public partial class BatAI : CharacterBody2D
{
	[Export]
	public float ChaseRange = 200f;
	[Export]
	public float Speed = 150f;

	private CharacterBody2D playa;

	public override void _Ready()
	{
		var rootNode = GetTree().Root;
		playa = rootNode.GetNode<CharacterBody2D>("World/Player");
	}

	public override void _Process(float delta)
	{
		if (playa != null)
		{
			float distanceToPlayer = GlobalPosition.DistanceTo(playa.GlobalPosition);

			if (distanceToPlayer <= ChaseRange)
			{
				Vector2 direction = (playa.GlobalPosition - GlobalPosition).Normalized();
				Vector2 targetPosition = GlobalPosition + direction * Speed * delta;

				MoveAndSlide(targetPosition - GlobalPosition);
			}
		}
	}
}
