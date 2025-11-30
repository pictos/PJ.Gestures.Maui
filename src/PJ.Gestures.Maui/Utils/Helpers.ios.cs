using UIKit;

namespace PJ.Gestures.Maui;

static partial class Helpers
{

	public static GestureStatus ToMauiStatus(this UIGestureRecognizerState state) => state switch
	{
		UIGestureRecognizerState.Began => GestureStatus.Started,
		UIGestureRecognizerState.Changed => GestureStatus.Running,
		UIGestureRecognizerState.Ended => GestureStatus.Completed,
		_ => GestureStatus.Canceled
	};

	public static Direction ToMauiDirection(this UISwipeGestureRecognizerDirection direction) => direction switch
	{
		UISwipeGestureRecognizerDirection.Right => Direction.Right,
		UISwipeGestureRecognizerDirection.Left => Direction.Left,
		UISwipeGestureRecognizerDirection.Up => Direction.Up,
		UISwipeGestureRecognizerDirection.Down => Direction.Down,
		_ => Direction.Unknown
	};
}