using Godot;
using System;

public class RegisterMenu : Node
{
	public override void _Ready()
	{
		GetNode<Button>("Register").Connect("pressed", this, nameof(OnRegisterPressed));
	}

	void OnRegisterPressed() {
		GetNode<AuthController>("/root/AuthController").Register(
			GetNode<LineEdit>("Email").Text,
			GetNode<LineEdit>("Password").Text
		);
	}
}
