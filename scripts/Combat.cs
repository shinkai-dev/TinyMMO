using Godot;
using System;

public class Combat : Node2D
{
    public RayCast2D rayCast;
    [Export]
    public float AttackRange = 200f;
    public Timer attackDelay;
    public Sprite attackCursor;

    public override void _Ready()
    {
        rayCast = new RayCast2D();
        rayCast.Enabled = true;
        rayCast.CollisionMask = 2;
        AddChild(rayCast);

        attackDelay = new Timer();
        AddChild(attackDelay);

        attackCursor = new Sprite();
        attackCursor.Texture = GD.Load<Texture>("res://assets/combat/attackCursor.png");
        attackCursor.Offset = new Vector2(6, 0);
        AddChild(attackCursor);
        attackCursor.ZIndex = 3;

        attackDelay.Connect("timeout", this, nameof(Attack));
    }

    public void Attack()
    {
        GD.Print("ATTACKED!");
        if (rayCast.IsColliding())
        {
            var target = rayCast.GetCollider() as Node;
            GD.Print("ATTACKED" + target.Name);
            var health = target?.GetNodeOrNull<Health>("Health");
            if (health != null)
            {
                health.TakeDamage(10);
            }
        }
    }
}