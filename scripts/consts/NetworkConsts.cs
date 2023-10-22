using Godot;
using System;

public abstract class NetworkConsts : Node
{
	public static readonly string IP = "127.0.0.1";
	public static readonly int PORT = 1234;
	public static readonly int MAX_PLAYERS = 4;
}
