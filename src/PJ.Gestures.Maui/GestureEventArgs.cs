namespace PJ.Gestures.Maui;

public sealed class PanEventArgs(Point[] touches, Vector2 distance, Rect viewPosition, Direction direction, GestureStatus status) : MotionEventArgs(touches, viewPosition, direction)
{
	public Vector2 Distance { get; } = distance;
	public GestureStatus GestureStatus { get; } = status;
}

public sealed class SwipeEventArgs(Point[] touches, Vector2 distance, Vector2 velocity, Rect viewPosition, Direction direction) : MotionEventArgs(touches, viewPosition, direction)
{
	public Vector2 Distance { get; } = distance;
	public Vector2 Velocity { get; } = velocity;
}

public sealed class LongPressEventArgs(Point touch, Rect viewPosition) : SingleTapEventArgs(touch, viewPosition);

public sealed class TapEventArgs(Point touch, Rect viewPosition) : SingleTapEventArgs(touch, viewPosition);

public abstract class MotionEventArgs(Point[] touches, Rect viewPosition, Direction direction) : BaseEventArgs(viewPosition)
{
	public Point[] Touches { get; } = touches;
	public Direction Direction { get; } = direction;
	public Point Center { get; } = GetCenter(touches);

}

public abstract class SingleTapEventArgs(Point touch, Rect viewPosition) : BaseEventArgs(viewPosition)
{
	public Point Touch { get; } = touch;
}

public abstract class BaseEventArgs(Rect viewPosition) : EventArgs
{
	public Rect ViewPosition { get; } = viewPosition;

	protected static Point GetCenter(Point[] points)
	{
		var size = points.Length;

		switch (size)
		{
			case 0: return Point.Zero;
			case 1: return points[0];
			default:
				double x = 0, y = 0;

				foreach (var point in points)
				{
					x += point.X;
					y += point.Y;
				}

				return new(x / size, y / size);
		}
	}
}