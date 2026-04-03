namespace PJ.Gestures.Maui;

public enum Direction : byte
{
	Unknown,
	Up,
	Down,
	Right,
	Left
}

public enum TouchStatus : byte
{
	Normal,
	HoverOver,
	Pressed,
}