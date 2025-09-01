using Android.Content;
using Android.Views;
using Microsoft.Maui.Platform;

namespace PJ.Gestures.Maui;
partial class GestureBehavior
{
	internal GestureDetector? gestureDetector;
	internal AView? PlatformView { get; private set; }

	protected override void OnAttachedTo(VisualElement bindable, AView platformView)
	{
		view = bindable;
		PlatformView = platformView;
		var context = platformView.Context;

		Assert(context is not null);

		gestureDetector = new GestureDetector(context, new SimpleGestureListener(this, context));
		platformView.Touch += OnPlatformTouch;
	}

	protected override void OnDetachedFrom(VisualElement bindable, AView platformView)
	{
		Assert(gestureDetector is not null, "GestureDetector shouldn't be null here!");
		platformView.Touch -= OnPlatformTouch;
		PlatformView = null;
		view = default!;
		gestureDetector.Dispose();
		gestureDetector = null;
	}

	void OnPlatformTouch(object? sender, AView.TouchEventArgs e)
	{
		Assert(gestureDetector is not null, "GestureDetector shouldn't be null here!");
		Assert(e.Event is not null);
		var @event = e.Event;

		gestureDetector.OnTouchEvent(@event);
		var motion = MotionEvent.Obtain(@event);

		motion?.Recycle();
	}

	public void HandleGestureFromParent(MotionEvent? motion)
	{
		if (!ReceiveGestureFromParent || motion is null)
		{
			return;
		}

		gestureDetector?.OnTouchEvent(motion);
	}


	//void HandleFlowGesture(MotionEvent? e)
	//{
	//	if (!FlowGesture || e is null)
	//		return;

	//	foreach (var b in visualElement.HandleGestureOnParentes())
	//	{
	//		b.gestureDetector?.OnTouchEvent(e);
	//	}
	//}

	public void FireTouchEvent(MotionEvent e)
	{
		ArgumentNullException.ThrowIfNull(e);

		gestureDetector?.OnTouchEvent(e);
	}
}

sealed class SimpleGestureListener : GestureDetector.SimpleOnGestureListener
{
	GestureBehavior behavior;
	readonly Context context;
	bool isScrolling;
	int scaledMaximumFlingVelocity;

	// Use this to move Views on the screen
	MotionEvent? Previous
	{
		get => field;
		set
		{
			field?.Recycle();
			field = value is null ? null : MotionEvent.Obtain(value);
		}
	}

	public SimpleGestureListener(GestureBehavior behavior, Context context)
	{
		ArgumentNullException.ThrowIfNull(behavior);
		ArgumentNullException.ThrowIfNull(context);
		this.behavior = behavior;
		this.context = context;

		var settings = ViewConfiguration.Get(context);
		Assert(settings is not null, "Settings shouldn't be null here.");

		scaledMaximumFlingVelocity = settings.ScaledMaximumFlingVelocity;
	}

	public override bool OnSingleTapConfirmed(MotionEvent e)
	{
		var args = GenerateTapEventArgs(e);
		behavior.TapFire(args);

		return base.OnSingleTapConfirmed(e);
	}

	public override void OnLongPress(MotionEvent e)
	{
		var args = GenerateLongPressEventArgs(e);
		behavior.LongPressFire(args);

		base.OnLongPress(e);
	}


	public override bool OnScroll(MotionEvent? e1, MotionEvent e2, float distanceX, float distanceY)
	{
		isScrolling = true;

		if (e1 is null)
		{
			return false;
		}

		var distance = new Vector2((float)context.FromPixels(distanceX), (float)context.FromPixels(distanceY));

		var touches = ComputeTouches(e2, context);

		// If Previous is null I should infer that we're handling the start gesture and it should be zeroed args
		var status = Previous is null ? e1.Action.ToGestureStatus() : e2.Action.ToGestureStatus();

		var direction = ComputeDirection(distanceX, distanceY);

		var args = new PanEventArgs(touches, distance, behavior.PlatformView.GetViewPosition(), direction, status);
		behavior.PanFire(args);

		if (e2.Action == MotionEventActions.Up)
		{
			HandleOnScrollUp(e2);
		}

		Previous = e2;

		return base.OnScroll(e1, e2, distanceX, distanceY);
	}

	public override bool OnFling(MotionEvent? e1, MotionEvent e2, float velocityX, float velocityY)
	{
		var relativeVx = velocityX / scaledMaximumFlingVelocity;
		var relativeVy = velocityY / scaledMaximumFlingVelocity;

		var swipedX = Math.Abs(relativeVx) > GestureBehavior.SwipeVelocityThreshold;
		var swipedY = Math.Abs(relativeVy) > GestureBehavior.SwipeVelocityThreshold;

		HandleOnScrollUp(e2);

		if (swipedX || swipedY)
		{
			var distance = Helpers.CalculateDistances(e1, e2, context);
			var velocity = new Vector2((float)context.FromPixels(velocityX), (float)context.FromPixels(velocityY));
			var touches = ComputeTouches(e2, context);
			var direction = ComputeSwipeDirection(velocityX, velocityY);

			var args = new SwipeEventArgs(touches, distance, velocity, behavior.PlatformView.GetViewPosition(), direction);
			behavior.SwipeFire(args);
		}

		isScrolling = false;
		Previous = null;

		return false;
	}

	public override bool OnDoubleTap(MotionEvent e)
	{
		var args = GenerateTapEventArgs(e);
		behavior.DoubleTapFire(args);
		return base.OnDoubleTap(e);
	}

	// This method is always fired after OnScroll interaction, based on the velocity of the gesture
	// which can be difficult to determine which one to use
	void HandleOnScrollUp(MotionEvent currentEvent)
	{
		if (!isScrolling)
		{
			return;
		}

		var cX = currentEvent.GetX();
		var cY = currentEvent.GetY();
		var pX = Previous?.GetX() ?? 0;
		var pY = Previous?.GetY() ?? 0;

		var dX = cX - pX;
		var dY = cY - pY;

		var distance = Helpers.CalculateDistances(currentEvent, Previous, context);
		var touches = ComputeTouches(currentEvent, context);
		var direction = ComputeDirection(dX, dY);

		var args = new PanEventArgs(touches, distance, behavior.PlatformView.GetViewPosition(), direction, GestureStatus.Completed);

		behavior.PanFire(args);

		isScrolling = false;
	}

	static Direction ComputeDirection(float dX, float dY)
	{
		Direction direction;

		if (Math.Abs(dX) > Math.Abs(dY))
		{
			direction = dX > 0 ? Direction.Left : Direction.Right;
		}
		else
		{
			direction = dY > 0 ? Direction.Up : Direction.Down;
		}

		return direction;
	}

	static Direction ComputeSwipeDirection(float dX, float dY)
	{
		Direction direction;

		if (Math.Abs(dX) > Math.Abs(dY))
		{
			direction = dX < 0 ? Direction.Left : Direction.Right;
		}
		else
		{
			direction = dY < 0 ? Direction.Up : Direction.Down;
		}

		return direction;
	}

	static Point[] ComputeTouches(MotionEvent current, Context context)
	{
		var pointers = current.PointerCount;
		var touches = new Point[pointers];
		var coordinates = new MotionEvent.PointerCoords();

		for (var i = 0; i < pointers; i++)
		{
			current.GetPointerCoords(i, coordinates);
			touches[i] = new((float)context.FromPixels(coordinates.X), (float)context.FromPixels(coordinates.Y));
		}

		return touches;
	}

	TapEventArgs GenerateTapEventArgs(MotionEvent e)
	{
		var point = GetPointsForSingleTap(e, context);
		return new(point, behavior.PlatformView.GetViewPosition());
	}

	LongPressEventArgs GenerateLongPressEventArgs(MotionEvent e)
	{
		var point = GetPointsForSingleTap(e, context);
		return new(point, behavior.PlatformView.GetViewPosition());
	}

	static Point GetPointsForSingleTap(MotionEvent e, Context context)
	{
		var x = context.FromPixels(e.GetX());
		var y = context.FromPixels(e.GetY());

		return new(x, y);
	}

	protected override void Dispose(bool disposing)
	{
		behavior = null!;
		base.Dispose(disposing);
	}
}
