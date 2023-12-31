using Godot;
using System;

public class MainMenu : Node
{
	private Button PlayButton;
	private Button HostButton;
	private Button RegisterButton;
	private LineEdit Email;
	private LineEdit Password;
	private PopupController PopupController;

	public override void _Ready()
	{
		PlayButton = GetNode<Button>("Play");
		HostButton = GetNode<Button>("Host");
		RegisterButton = GetNode<Button>("Register");
		Email = GetNode<LineEdit>("Email");
		Password = GetNode<LineEdit>("Password");
		PopupController = GetNode<PopupController>("/root/PopupController");

		PlayButton.Connect("pressed", this, nameof(OnPlayPressed));
		HostButton.Connect("pressed", this, nameof(OnHostPressed));
		RegisterButton.Connect("pressed", this, nameof(OnRegisterPressed));
	}

	async void OnPlayPressed()
	{
		var email = Email.Text;
		var password = Password.Text;
		ToggleForm(false);
		PopupController.ShowLoading();
		await GetNode<AuthController>("/root/AuthController").Login(email, password);
		PopupController.HideLoading();
		ToggleForm(true);
	}

	void OnHostPressed()
	{
		var peer = new NetworkedMultiplayerENet();
		var error = peer.CreateServer(NetworkConsts.PORT, NetworkConsts.MAX_PLAYERS);
		if (error != Error.Ok)
		{
			_ = PopupController.ShowMessage("Error", "Could not create server. Verify that the port is not in use.");
			return;
		}
		GetTree().NetworkPeer = peer;
		OpenGame();
	}

	void OnRegisterPressed()
	{
		GetTree().ChangeScene("res://scenes/menu/Register.tscn");
	}

	void OpenGame()
	{
		GetTree().ChangeScene("res://scenes/GameController.tscn");
	}

	void ToggleForm(bool enabled)
	{
		Email.Editable = enabled;
		Password.Editable = enabled;
		PlayButton.Disabled = !enabled;
		HostButton.Disabled = !enabled;
		RegisterButton.Disabled = !enabled;
	}
}
