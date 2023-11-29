using Godot;
using DiscordRPC;

public class DiscordRich : Node2D
{
	// Declare a Discord RichPresence object
	public DiscordRpcClient client;
	public override void _Ready()
	{
		client = new DiscordRpcClient("1168348162306625600");
		client.OnReady += (sender, msg) => {
			GD.Print("Received Ready from User {0}", msg.User.Username);
		};

		client.OnPresenceUpdate += (sender, e) => {
			GD.Print("Received Update! {0}", e.Presence);
		};

		client.Initialize();
		GD.Print("Iniciado");
		client.SetPresence(new RichPresence() //Isso aqui seta o RichPresence no usuário
		{
			Details = "Teste de detalhe",
			State = "teste de estado",
			Assets = new Assets() //Isso aqui inicia os assets(Ícone e etc)
			{
				LargeImageKey = "icon", //isso aqui é o ícone de um arquivo que existe no RPC
				LargeImageText = "icon",
				SmallImageKey = "icon"
			}
		});
	}
}
