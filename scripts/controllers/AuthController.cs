using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class AuthController : Node
{
	private string Token;
	private HttpController HttpController;
	private PopupController PopupController;
	private UserCollection UserCollection;

	public override void _Ready()
	{
		HttpController = GetNode<HttpController>("/root/HttpController");
		PopupController = GetNode<PopupController>("/root/PopupController");
		UserCollection = new UserCollection();
	}

	public string GetToken()
	{
		return Token + "";
	}

	public async Task Register(string email, string password)
	{
		var body =
			new Godot.Collections.Dictionary()
			{
				{ "email", email },
				{ "password", password },
				{ "returnSecureToken", true }
			};
		var response = await HttpController.Post(EndpointConsts.FIREBASE_REGISTER, body);
		if (response.StatusCode == 200)
		{
			Token = response.Body["refreshToken"].ToString();
			await PopupController.ShowMessage("Success", "You have been registered successfully!");
			GoToGame();
		}
		else
		{
			var rawError = ((Dictionary)response.Body["error"])["message"].ToString();
			var error = ErrorMessageConsts.dictionary.ContainsKey(rawError) ? ErrorMessageConsts.dictionary[rawError] : rawError;
			_ = PopupController.ShowMessage("Error", error);
		}
	}

	public async Task Login(string email, string password)
	{
		var body =
			new Godot.Collections.Dictionary()
			{
				{ "email", email },
				{ "password", password },
				{ "returnSecureToken", true }
			};
		var response = await HttpController.Post(EndpointConsts.FIREBASE_LOGIN, body);

		if (response.StatusCode == 200)
		{
			Token = response.Body["refreshToken"].ToString();
			GoToGame();
		}
		else
		{
			var rawError = ((Dictionary)response.Body["error"])["message"].ToString();
			var error = ErrorMessageConsts.dictionary.ContainsKey(rawError) ? ErrorMessageConsts.dictionary[rawError] : rawError;
			_ = PopupController.ShowMessage("Error", error);
		}
	}

	void GoToGame()
	{
		var peer = new ENetMultiplayerPeer();
		peer.CreateClient(NetworkConsts.IP, NetworkConsts.PORT);
		GetTree().NetworkPeer = peer;
		GetTree().ChangeSceneToFile("res://scenes/GameController.tscn");
	}

	public void CreateCharacter(string name)
	{
		RpcId(1, nameof(AddCharacterToDb), GetTree().GetUniqueId(), Token, name);
	}

	[RPC]
	void ShowError(string error)
	{
		_ = PopupController.ShowMessage("Error", error);
		PopupController.HideLoading();
	}

	The master and mastersync rpc behavior is not officially supported anymore. Try using another keyword or making custom logic using Multiplayer.GetRemoteSenderId()
[RPC]
	async void AddCharacterToDb(int networkId, string token, string name)
	{
		if (GetTree().GetUniqueId() != 1)
		{
			return;
		}

		var playersWithName = UserCollection.QueryBy("name", name);
		if (playersWithName.Length > 0)
		{
			RpcId(networkId, nameof(ShowError), "Name already taken");
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
		UserCollection.SetDoc(
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
		RpcId(networkId, nameof(ToggleLoading));
	}

	[RPC]
	void ToggleLoading()
	{
		PopupController.HideLoading();
	}
}
