using Godot;
using System;
using System.Collections.Generic;

public class Dialogue : CanvasLayer
{
	[Export]
	public string d_file = "res://test.json"; // caminho do arquivo JSON

	private List<Dictionary<string, string>> dialogue;
	private int currentDialogueIndex = 0;

	public override void _Ready()
	{
		LoadDialog();
		Start();
	}

	public void Start()
	{
		var labelName = GetNode<RichTextLabel>("Name");
		var labelMsg = GetNode<RichTextLabel>("Message");
		labelName.Text = dialogue[currentDialogueIndex]["name"];
		labelMsg.Text = dialogue[currentDialogueIndex]["text"];
	}

	public void LoadDialog()
	{
		var file = new File();
		if (file.FileExists(d_file))
		{
			file.Open(d_file, File.ModeFlags.Read);
			var text = file.GetAsText();
			file.Close();

			dialogue = new List<Dictionary<string, string>>();
			var parsed = JSON.Parse(text).Result as Array;
			foreach (var item in parsed)
			{
				var node = item as Dictionary<string, object>;
				var entry = new Dictionary<string, string>();
				foreach (var key in node.Keys)
				{
					entry[key] = node[key].ToString();
				}
				dialogue.Add(entry);
			}
		}
	}

	public void NextDialogue()
	{
		currentDialogueIndex++;
		if (currentDialogueIndex < dialogue.Count)
		{
			var labelName = GetNode<RichTextLabel>("Nome");
			var labelMsg = GetNode<RichTextLabel>("Message");
			labelName.Text = dialogue[currentDialogueIndex]["name"];
			labelMsg.Text = dialogue[currentDialogueIndex]["text"];
		}
		else
		{
			// Fim do diálogo
			QueueFree();
		}
	}
}
