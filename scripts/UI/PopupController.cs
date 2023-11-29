using Godot;
using System;
using System.Threading.Tasks;

public class PopupController : Node
{
    private AcceptDialog GamePopup;
    private Control Loading;
    [Signal] public delegate bool PopupToggled();

    public override void _Ready()
    {
        var canvas = (PackedScene)ResourceLoader.Load("res://scenes/menu/ContainerCanvas.tscn");
        var containerCanvas = (CanvasLayer)canvas.Instance();
        GetTree().Root.CallDeferred("add_child", containerCanvas);

        var gamePopupScene = (PackedScene)ResourceLoader.Load("res://scenes/menu/GamePopup.tscn");
        GamePopup = (AcceptDialog)gamePopupScene.Instance();
        containerCanvas.AddChild(GamePopup);

        var loadingScene = (PackedScene)ResourceLoader.Load("res://scenes/menu/Loading.tscn");
        Loading = (Control)loadingScene.Instance();
        Loading.Visible = false;
        containerCanvas.AddChild(Loading);
    }

    public async Task ShowMessage(string title, string description) {
        GamePopup.WindowTitle = title;
        GamePopup.DialogText = description;
        GamePopup.PopupCentered();
        GamePopup.Raise();
        await ToSignal(GamePopup, "confirmed");
    }

    public void ShowLoading() {
        Loading.Visible = true;
        Loading.Raise();
        EmitSignal(nameof(PopupToggled), true);
    }

    public void HideLoading() {
        Loading.Visible = false;
        EmitSignal(nameof(PopupToggled), false);
    }
    
    public bool IsLoadingVisible() {
        return Loading.Visible;
    }
}
