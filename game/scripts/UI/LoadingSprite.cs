using Godot;
using System;

public partial class LoadingSprite : Sprite2D
{

	public override void _Process(double delta)
	{
		Rotate(0.1f);
	}
}
