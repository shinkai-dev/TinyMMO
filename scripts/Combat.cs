using Godot;
using System;

public class Combat : Node2D
{
    public RayCast2D rayCast;
    [Export]
    public float AttackRange = 30f;
    public Timer attackDelay;
    public Sprite attackCursor;

    public Sprite Hit;

    public override void _Ready()
    {
        rayCast = new RayCast2D();
        rayCast.Enabled = true;
        rayCast.CollisionMask = 2;
        rayCast.CastTo = new Vector2(AttackRange, 0);
        rayCast.AddException(Owner);
        AddChild(rayCast);

        attackDelay = new Timer();
        AddChild(attackDelay);

        attackCursor = new Sprite();
        attackCursor.Texture = GD.Load<Texture>("res://assets/combat/attackCursor.png");
        attackCursor.Offset = new Vector2(12, 0);
        AddChild(attackCursor);
        attackCursor.ZIndex = 3;

        Hit = new Sprite();
        Hit.Texture = GD.Load<Texture>("res://assets/combat/hit.png");
        Hit.ZIndex = 3;
        Hit.Visible = false;


        attackDelay.Connect("timeout", this, nameof(Attack));
    }

    public void Attack()
    {
        GD.Print("ATTACKED!");
        GD.Print("Raycast position:" + rayCast.CastTo);
        if (rayCast.IsColliding())
        {
            var target = rayCast.GetCollider() as Node;
            GD.Print("ATTACKED " + target.Name);
            var health = target?.GetNodeOrNull<Health>("Health");
            if (health != null)
            {
                health.TakeDamage(10);
                var bloodEmitter = target?.GetNodeOrNull<CPUParticles2D>("BloodEmitter");
                bloodEmitter.Emitting = true;
                if(target.GetType() == typeof(KinematicBody2D))
                {
                    var targetBody = target as KinematicBody2D;
                    var direction = (targetBody.GlobalPosition - GlobalPosition).Normalized();
                    bloodEmitter.Angle = Mathf.Rad2Deg(direction.Angle());
                    bloodEmitter.Direction = direction * 50f;
                }
            }
            GD.Print((GetGlobalMousePosition() - GlobalPosition).Normalized() * AttackRange);
        }
    }
}