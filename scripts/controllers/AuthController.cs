using Godot;
using Godot.Collections;
using System.Text;

public partial class AuthController : Node
{
	private HTTPRequest RegisterRequest;
	private HTTPRequest LoginRequest;
	private string Token;

	public override void _Ready()
	{
		RegisterRequest = new HTTPRequest();
		RegisterRequest.Connect("request_completed", this, nameof(OnRegisterCompleted));
		AddChild(RegisterRequest);
		LoginRequest = new HTTPRequest();
		LoginRequest.Connect("request_completed", this, nameof(OnLoginCompleted));
		AddChild(LoginRequest);
	}

	public string GetToken()
	{
		return Token + "";
	}

	public void Register(string email, string password)
	{
		var body = JSON.Print(
			new Dictionary()
			{
				{ "email", email },
				{ "password", password },
				{ "returnSecureToken", true }
			}
		);
		var headers = new string[]
		{
			"Content-Type: application/json",
			"Content-Length: " + body.Length
		};
		RegisterRequest.Request(EndpointConsts.FIREBASE_REGISTER, headers, true, HTTPClient.Method.Post, body);
	}

	public void Login(string email, string password)
	{
		var body = JSON.Print(
			new Dictionary()
			{
				{ "email", email },
				{ "password", password },
				{ "returnSecureToken", true }
			}
		);
		var headers = new string[]
		{
			"Content-Type: application/json",
			"Content-Length: " + body.Length
		};
		LoginRequest.Request(EndpointConsts.FIREBASE_LOGIN, headers, true, HTTPClient.Method.Post, body);
	}

	void OnRegisterCompleted(int result, int responseCode, string[] headers, byte[] body)
	{
		var response = JSON.Parse(Encoding.UTF8.GetString(body)).Result as Dictionary;
		var gamePopupScene = (PackedScene)ResourceLoader.Load("res://scenes/menu/GamePopup.tscn");
		var gamePopup = (AcceptDialog)gamePopupScene.Instance();
		if (responseCode == 200)
		{
			Token = response["refreshToken"].ToString();
			gamePopup.WindowTitle = "Register";
			gamePopup.DialogText = "You have been registered successfully!";
			gamePopup.Connect("confirmed", this, nameof(GoToGame));
			GetTree().Root.AddChild(gamePopup);
		}
		else
		{
			gamePopup.WindowTitle = "Error";
			var rawError = ((Dictionary)response["error"])["message"].ToString();
			gamePopup.DialogText = ErrorMessageConsts.dictionary.ContainsKey(rawError) ? ErrorMessageConsts.dictionary[rawError] : rawError;
			GetTree().Root.AddChild(gamePopup);
		}
		gamePopup.Show();
	}

	void OnLoginCompleted(int result, int responseCode, string[] headers, byte[] body)
	{
		var response = JSON.Parse(Encoding.UTF8.GetString(body)).Result as Dictionary;
		if (responseCode == 200)
		{
			Token = response["refreshToken"].ToString();
			GoToGame();
		}
		else
		{
			var gamePopupScene = (PackedScene)ResourceLoader.Load("res://scenes/menu/GamePopup.tscn");
			var gamePopup = (AcceptDialog)gamePopupScene.Instance();
			gamePopup.WindowTitle = "Error";
			var rawError = ((Dictionary)response["error"])["message"].ToString();
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
	void AddCharacterToDb(int networkId, string token, string name)
	{
		if (GetTree().GetNetworkUniqueId() != 1)
		{
			return;
		}

		var tokenCheckRequest = new HTTPRequest();
		tokenCheckRequest.Connect("request_completed", this, nameof(FinishCharacterCreation), new Godot.Collections.Array() { networkId, name });
		AddChild(tokenCheckRequest);
		var body = JSON.Print(
			new Godot.Collections.Dictionary()
			{
				{ "grant_type", "refresh_token" },
				{ "refresh_token", token },
			}
		);
		var headers = new string[]
		{
			"Content-Type: application/json",
			"Content-Length: " + body.Length
		};
		tokenCheckRequest.Request(EndpointConsts.TOKEN_CHECK, headers, true, HTTPClient.Method.Post, body);
	}

	[Master]
	void FinishCharacterCreation(int result, int responseCode, string[] headers, byte[] body, int networkId, string name)
	{
		if (GetTree().GetNetworkUniqueId() != 1)
		{
			return;
		}

		var response = JSON.Parse(Encoding.UTF8.GetString(body)).Result as Godot.Collections.Dictionary;
		GD.Print(response);
		var uid = response["user_id"] as string;
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
