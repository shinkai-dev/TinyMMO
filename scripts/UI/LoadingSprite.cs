using Godot;
using System;

public partial class LoadingSprite : Sprite2D
{

	public override void _Process(float delta)
	{
		Rotate(0.1f);
	}
}
