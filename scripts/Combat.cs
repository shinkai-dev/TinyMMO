using Godot;
using System;

public class Combat : Node2D
{
    public RayCast2D rayCast;
    [Export]
    public float AttackRange = 30f;
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
        attackCursor = new Sprite();
        attackCursor.Texture = GD.Load<Texture>("res://assets/combat/attackCursor.png");
        attackCursor.Offset = new Vector2(12, 0);
        AddChild(attackCursor);
        attackCursor.ZIndex = 3;

        AddChild(Hit);
    }
    public void HideHit()
    {
        Hit.Visible = false;
    }
    public void Attack()
    {
        var attackTimer = Owner?.GetNodeOrNull<Timer>("attackTimer");
        if(attackTimer.TimeLeft > 0)
        {
            return;
        }
        else{
            attackTimer.Start();
        }
        GD.Print("ATTACKED!");
        GD.Print("Raycast position:" + rayCast.CastTo);
        if (rayCast.IsColliding())
        {
            var target = rayCast.GetCollider() as Node;
            GD.Print("ATTACKED " + target.Name);
            var attackSound = Owner?.GetNodeOrNull<AudioStreamPlayer2D>("attackSound");
            var health = target?.GetNodeOrNull<Health>("Health");
            if (health != null)
            {
                health.TakeDamage(10);
                attackSound.Play();
                var bloodEmitter = target?.GetNodeOrNull<CPUParticles2D>("BloodEmitter");
                var hitEmitter = target?.GetNodeOrNull<CPUParticles2D>("HitEmitter");
                bloodEmitter.Emitting = true;
                hitEmitter.Emitting = true;
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