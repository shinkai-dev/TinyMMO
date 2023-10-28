using Godot;
using System;

public class BatAI : KinematicBody2D
{
	[Export]
	public float ChaseRange = 200f;
	[Export]
	public float Speed = 150f;
    private Health health;
    private Combat combat;
	private KinematicBody2D playa;

	public override void _Ready()
	{
		var rootNode = GetTree().Root;
		playa = rootNode.GetNode<KinematicBody2D>("World/Player");
        health = GetNode<Health>("Health");
        combat = GetNode<Combat>("Combat");
		GetNode<Health>("Health").Connect("Dead", this, nameof(OnNPCDead));
	}


	public void OnNPCDead()
    {
		GD.Print(Name + " dead");
		QueueFree();
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
