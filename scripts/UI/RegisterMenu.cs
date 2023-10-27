using Godot;
using System;

public class RegisterMenu : Node
{
	public override void _Ready()
	{
		GetNode<Button>("Register").Connect("pressed", this, nameof(OnRegisterPressed));
		GetNode<Button>("Return").Connect("pressed", this, nameof(onReturnPressed));
	}

	void OnRegisterPressed() {
		GetNode<AuthController>("/root/AuthController").Register(
			GetNode<LineEdit>("Email").Text,
			GetNode<LineEdit>("Password").Text
		);
	}
	void onReturnPressed() {
		GetTree().ChangeScene("res://scenes/menu/menuPrin.tscn");
	}
}
