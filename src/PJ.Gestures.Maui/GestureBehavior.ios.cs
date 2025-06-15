using CoreGraphics;
using UIKit;

namespace PJ.Gestures.Maui;

sealed class CustomGestureRecognizerDelegate : UIGestureRecognizerDelegate
{
	public override bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
	{
		return true;
	}
}

partial class GestureBehavior
{
	static readonly CustomGestureRecognizerDelegate multipleTouchesDelegate = new();
	CGPoint previous = CGPoint.Empty;
	Direction previousPanDirection = Direction.Unknown;

	readonly UITapGestureRecognizer tapGestureRecognizer;
	readonly UITapGestureRecognizer doubleTapGestureRecognizer;
	readonly UIPanGestureRecognizer panGestureRecognizer;
	readonly UILongPressGestureRecognizer longPressGestureRecognizer;

	public GestureBehavior()
	{
		tapGestureRecognizer = new(SingleTapHandler);
		doubleTapGestureRecognizer = new(DoubleTapHandler);
		panGestureRecognizer = new(PanGestureHandler);
		longPressGestureRecognizer = new(LongPressHandler);
	}

	protected override void OnAttachedTo(VisualElement bindable, UIView platformView)
	{
		if (FlowGesture)
		{
			doubleTapGestureRecognizer.Delegate = multipleTouchesDelegate;
			panGestureRecognizer.Delegate = multipleTouchesDelegate;
			longPressGestureRecognizer.Delegate = multipleTouchesDelegate;
		}
		if (Tap?.GetInvocationList()?.Length > 0)
		{
			platformView.AddGestureRecognizer(tapGestureRecognizer);
		}

		if (DoubleTap?.GetInvocationList()?.Length > 0)
		{
			platformView.AddGestureRecognizer(doubleTapGestureRecognizer);
		}
		if (Pan?.GetInvocationList()?.Length > 0 || Swipe?.GetInvocationList().Length > 0)
		{
			platformView.AddGestureRecognizer(panGestureRecognizer);
		}
		if (LongPress?.GetInvocationList()?.Length > 0)
		{
			platformView.AddGestureRecognizer(longPressGestureRecognizer);
		}
	}

	protected override void OnDetachedFrom(VisualElement bindable, UIView platformView)
	{
		if (tapGestureRecognizer is not null)
			platformView.RemoveGestureRecognizer(tapGestureRecognizer);

		if (doubleTapGestureRecognizer is not null)
			platformView.RemoveGestureRecognizer(doubleTapGestureRecognizer);

		if (panGestureRecognizer is not null)
			platformView.RemoveGestureRecognizer(panGestureRecognizer);

		if (longPressGestureRecognizer is not null)
			platformView.RemoveGestureRecognizer(longPressGestureRecognizer);
	}

	void LongPressHandler(UILongPressGestureRecognizer gesture)
	{
		if (gesture.State != UIGestureRecognizerState.Began)
			return;

		var view = gesture.View;
		var rect = CalculateViewPosition(view);
		var touch = CalculateTouch(gesture, view);

		var args = new LongPressEventArgs(touch, rect);

		LongPressFire(args);
	}

	void SingleTapHandler(UITapGestureRecognizer gesture)
	{
		var view = gesture.View;
		var rect = CalculateViewPosition(view);
		var touch = CalculateTouch(gesture, view);

		cts.Dispose();
		cts = RegisterNewCts();

		Task.Run(async () => await doubleTapCompletionSource.Task.WaitAsync(cts.Token)).ContinueWith(t =>
		{
			Assert(MainThread.IsMainThread, "It should run on UI Thread!");

			if (t.Status == TaskStatus.Canceled)
			{
				var args = new TapEventArgs(touch, rect);
				TapFire(args);
			}
		}, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
	}

	void DoubleTapHandler(UITapGestureRecognizer gesture)
	{
		doubleTapCompletionSource.SetResult();
		doubleTapCompletionSource = new();

		var view = gesture.View;
		var rect = CalculateViewPosition(view);
		var touch = CalculateTouch(gesture, view);

		var args = new TapEventArgs(touch, rect);
		DoubleTapFire(args);
	}

	void PanGestureHandler(UIPanGestureRecognizer gesture)
	{
		var status = gesture.State.ToMauiStatus();
		var view = gesture.View;
		var translation = gesture.TranslationInView(view);
		var velocity = gesture.VelocityInView(view);
		var distance = new Vector2((float)translation.X, (float)translation.Y);

		var touches = CalculateTouches(gesture, view);
		var rect = CalculateViewPosition(view);
		var direction = Direction.Unknown;

		if (status is GestureStatus.Completed or GestureStatus.Canceled & HandlesSwipe)
		{
			if (previous == CGPoint.Empty)
			{
				return;
			}

			var swipeVelocityThreshold = SwipeVelocityThreshold * 8_000;
			var isSwipeX = (float)Math.Abs(velocity.X) > swipeVelocityThreshold;
			var isSwipeY = (float)Math.Abs(velocity.Y) > swipeVelocityThreshold;

			if (isSwipeX || isSwipeY)
			{
				if (isSwipeX)
				{
					direction = velocity.X > 0 ? Direction.Right : Direction.Left;
				}
				else if (isSwipeY)
				{
					direction = velocity.Y > 0 ? Direction.Down : Direction.Up;
				}

				var panArgs = new PanEventArgs(touches, distance, rect, direction, GestureStatus.Canceled);
				PanFire(panArgs);

				var swipeArgs = new SwipeEventArgs(touches, distance, new((float)velocity.X, (float)velocity.Y), rect, direction);

				SwipeFire(swipeArgs);

				gesture.CancelsTouchesInView = true;

				goto FINISH;
			}
		}

		if (status is GestureStatus.Completed)
		{
			direction = previousPanDirection;
			previousPanDirection = Direction.Unknown;
		}
		else
		{
			direction = CalculateDirection(translation, previous);
		}

		var args = new PanEventArgs(touches, distance, rect, direction, status);
		PanFire(args);
		previousPanDirection = direction;
		previous = translation;

		FINISH:
		if (gesture.State is UIGestureRecognizerState.Ended or UIGestureRecognizerState.Cancelled)
		{
			gesture.SetTranslation(CGPoint.Empty, view);
			previous = CGPoint.Empty;
		}
	}

	static Rect CalculateViewPosition(UIView view)
	{
		var viewBounds = view.Bounds;

		return new(viewBounds.X, viewBounds.Y, viewBounds.Width, viewBounds.Height);
	}

	static Point CalculateTouch(UIGestureRecognizer gesture, UIView view)
	{
		var location = gesture.LocationInView(view);
		return new(location.X, location.Y);
	}

	static Point[] CalculateTouches(UIGestureRecognizer gesture, UIView view)
	{
		var numberOfTouches = gesture.NumberOfTouches;

		if (numberOfTouches <= 1)
		{
			var point = CalculateTouch(gesture, view);
			return [point];
		}

		var points = new Point[numberOfTouches];

		for (var i = 0; i < numberOfTouches; i++)
		{
			var location = gesture.LocationOfTouch(i, view);
			points[i] = new(location.X, location.Y);
		}

		return points;
	}

	static Direction CalculateDirection(CGPoint translation, CGPoint previous)
	{
		var dX = translation.X - previous.X;
		var dY = translation.Y - previous.Y;

		if (Math.Abs(dX) > Math.Abs(dY))
		{
			return dX > 0 ? Direction.Right : Direction.Left;
		}

		return dY > 0 ? Direction.Down : Direction.Up;
	}
}