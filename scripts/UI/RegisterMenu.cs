using Godot;
using System;

public class RegisterMenu : Node
{
	private Button RegisterButton;
	private Button ReturnButton;
	private AuthController AuthController;
	private PopupController PopupController;
	private LineEdit Email;
	private LineEdit Password;

	public override void _Ready()
	{
		RegisterButton = GetNode<Button>("Register");
		ReturnButton = GetNode<Button>("Return");
		AuthController = GetNode<AuthController>("/root/AuthController");
		PopupController = GetNode<PopupController>("/root/PopupController");
		Email = GetNode<LineEdit>("Email");
		Password = GetNode<LineEdit>("Password");
		RegisterButton.Connect("pressed", this, nameof(OnRegisterPressed));
		ReturnButton.Connect("pressed", this, nameof(onReturnPressed));
	}

	async void OnRegisterPressed()
	{
		ToggleForm(false);
		PopupController.ShowLoading();
		await AuthController.Register(Email.Text, Password.Text);
		PopupController.HideLoading();
		ToggleForm(true);
	}

	void ToggleForm(bool enabled)
	{
		Email.Editable = enabled;
		Password.Editable = enabled;
		RegisterButton.Disabled = !enabled;
		ReturnButton.Disabled = !enabled;
	}

	void onReturnPressed()
	{
		GetTree().ChangeScene("res://scenes/menu/menuPrin.tscn");
	}
}
