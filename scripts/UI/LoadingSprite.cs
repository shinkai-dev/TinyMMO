using Godot;
using System;

public class LoadingSprite : Sprite
{

	public override void _Process(float delta)
	{
		Rotate(0.1f);
	}
}
