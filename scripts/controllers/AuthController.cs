using Godot;
using Godot.Collections;

public partial class AuthController : Node
{
	private string Token;

	public string GetToken()
	{
		return Token + "";
	}

	public async void Register(string email, string password)
	{
		var httpController = GetNode<HttpController>("/root/HttpController");
		var body = 
			new Godot.Collections.Dictionary()
			{
				{ "email", email },
				{ "password", password },
				{ "returnSecureToken", true }
			};
		var response = await httpController.Post(EndpointConsts.FIREBASE_REGISTER, body);
		var gamePopupScene = (PackedScene)ResourceLoader.Load("res://scenes/menu/GamePopup.tscn");
		var gamePopup = (AcceptDialog)gamePopupScene.Instance();
		if (response.StatusCode == 200)
		{
			Token = response.Body["refreshToken"].ToString();
			gamePopup.WindowTitle = "Register";
			gamePopup.DialogText = "You have been registered successfully!";
			gamePopup.Connect("confirmed", this, nameof(GoToGame));
			GetTree().Root.AddChild(gamePopup);
		}
		else
		{
			gamePopup.WindowTitle = "Error";
			var rawError = ((Dictionary)response.Body["error"])["message"].ToString();
			gamePopup.DialogText = ErrorMessageConsts.dictionary.ContainsKey(rawError) ? ErrorMessageConsts.dictionary[rawError] : rawError;
			GetTree().Root.AddChild(gamePopup);
		}
		gamePopup.Show();
	}

	public async void Login(string email, string password)
	{
		var httpController = GetNode<HttpController>("/root/HttpController");
		var body = 
			new Godot.Collections.Dictionary()
			{
				{ "email", email },
				{ "password", password },
				{ "returnSecureToken", true }
			};
		var response = await httpController.Post(EndpointConsts.FIREBASE_LOGIN, body);

		if (response.StatusCode == 200)
		{
			Token = response.Body["refreshToken"].ToString();
			GoToGame();
		}
		else
		{
			var gamePopupScene = (PackedScene)ResourceLoader.Load("res://scenes/menu/GamePopup.tscn");
			var gamePopup = (AcceptDialog)gamePopupScene.Instance();
			gamePopup.WindowTitle = "Error";
			var rawError = ((Dictionary)response.Body["error"])["message"].ToString();
			gamePopup.DialogText = ErrorMessageConsts.dictionary.ContainsKey(rawError) ? ErrorMessageConsts.dictionary[rawError] : rawError;
			GetTree().Root.AddChild(gamePopup);
			gamePopup.Show();
		}
	}

	void GoToGame()
	{
		var peer = new NetworkedMultiplayerENet();
		peer.CreateClient(NetworkConsts.IP, NetworkConsts.PORT);
		GetTree().NetworkPeer = peer;
		GetTree().ChangeScene("res://scenes/GameController.tscn");
	}

	public void CreateCharacter(string name)
	{
		RpcId(1, nameof(AddCharacterToDb), GetTree().GetNetworkUniqueId(), Token, name);
	}

	[Master]
	async void AddCharacterToDb(int networkId, string token, string name)
	{
		if (GetTree().GetNetworkUniqueId() != 1)
		{
			return;
		}

		var body = 
			new Godot.Collections.Dictionary()
			{
				{ "grant_type", "refresh_token" },
				{ "refresh_token", token },
			};
		var httpController = GetNode<HttpController>("/root/HttpController");
		var response = await httpController.Post(EndpointConsts.TOKEN_CHECK, body);
		var uid = response.Body["user_id"] as string;
		var userCollection = new UserCollection();
		userCollection.SetDoc(
			new UserModel()
			{
				Id = uid,
				Name = name,
				Color = new ColorModel()
				{
					r = GD.Randf(),
					g = GD.Randf(),
					b = GD.Randf()
				}
			}
		);
		RpcId(networkId, nameof(SendToGame));
	}

	[Master]
	void SendToGame()
	{
		GetTree().ChangeScene("res://scenes/GameController.tscn");
	}
}
