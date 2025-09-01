using System.Diagnostics;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using PJ.Gestures.Maui.Utils;
using WGestureRecognizer = Microsoft.UI.Input.GestureRecognizer;
using WGestureSettings = Microsoft.UI.Input.GestureSettings;
using WWindow = Microsoft.UI.Xaml.Window;

namespace PJ.Gestures.Maui;

partial class GestureBehavior
{
	IMauiContext mauiContext = default!;
	FrameworkElement touchableView = default!;
	readonly WGestureRecognizer gestureRecognizer = new();
	WWindow Window => mauiContext.Services.GetRequiredService<WWindow>();
	bool isDoubleTap,
		isScrolling,
		isGestureSucceed;


	public GestureBehavior()
	{
		gestureRecognizer.GestureSettings = WGestureSettings.Tap |
			WGestureSettings.Hold |
			WGestureSettings.HoldWithMouse |
			WGestureSettings.ManipulationTranslateX |
			WGestureSettings.ManipulationTranslateY |
			WGestureSettings.RightTap;
	}

	protected override void OnAttachedTo(VisualElement bindable, FrameworkElement platformView)
	{
		mauiContext = bindable.Handler!.MauiContext ?? throw new NullReferenceException("MauiContext can't be null here.");
		touchableView = platformView;
		view = bindable;

		gestureRecognizer.ManipulationStarted += OnManipulationStarted;
		gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
		gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;
		gestureRecognizer.Holding += OnHolding;
		gestureRecognizer.Tapped += OnTapped;
		gestureRecognizer.RightTapped += OnRightTapped;

		platformView.DoubleTapped += OnDoubleTapped;
		platformView.PointerPressed += OnPointerPressed;
		platformView.PointerMoved += OnPointerMoved;
		platformView.PointerReleased += OnPointerReleased;
		platformView.PointerCanceled += OnPointerCanceled;
	}

	void OnPointerCanceled(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		if (!e.Pointer.IsInRange)
		{
			return;
		}

		var uiElement = (FrameworkElement?)sender ?? touchableView;

		if (uiElement is null)
		{
			return;
		}

		gestureRecognizer.CompleteGesture();
		uiElement.ReleasePointerCapture(e.Pointer);

		if (!isGestureSucceed && isScrolling)
		{
			var touch = e.GetCurrentPoint(uiElement).Position.ToMauiPoint();
			var rect = CalculateElementRect(uiElement);
			var arg = new PanEventArgs([touch], Vector2.Zero, rect, Direction.Unknown, GestureStatus.Canceled);
			PanFire(arg);
			isScrolling = false;
		}

		isGestureSucceed = false;
	}

	void OnPointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		if (!e.Pointer.IsInRange)
		{
			return;
		}

		var uiElement = (FrameworkElement?)sender ?? touchableView;

		if (uiElement is null)
		{
			return;
		}

		isGestureSucceed = true;

		try
		{
			gestureRecognizer.ProcessUpEvent(e.GetCurrentPoint(uiElement));
			uiElement.ReleasePointerCapture(e.Pointer);
		}
		catch (Exception ex)
		{
			Trace.WriteLine($"Error: {ex}: {ex.Message}");
		}
	}

	void OnPointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		if (!e.Pointer.IsInRange)
		{
			return;
		}

		var uiElement = (FrameworkElement?)sender ?? touchableView;

		if (uiElement is null)
		{
			return;
		}

		try
		{
			gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(uiElement));
		}
		catch (Exception ex)
		{
			Trace.WriteLine($"Error: {ex}: {ex.Message}");
		}
	}

	void OnPointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		if (!e.Pointer.IsInRange)
		{
			return;
		}

		var uiElement = (FrameworkElement?)sender ?? touchableView;

		if (uiElement is null)
		{
			return;
		}

		try
		{
			uiElement.CapturePointer(e.Pointer);
			isGestureSucceed = false;
			var point = e.GetCurrentPoint(uiElement);
			gestureRecognizer.ProcessDownEvent(point);
		}
		catch (ArgumentException ex)
		{
			Trace.WriteLine($"Error: {ex}: {ex.Message}");
		}
		e.Handled = true;
	}

	void OnManipulationCompleted(WGestureRecognizer sender, ManipulationCompletedEventArgs args)
	{
		var dX = args.Cumulative.Translation.X;
		var dY = args.Cumulative.Translation.Y;

		var distance = args.Cumulative.ToMauiDistance();
		var touch = args.Position.ToMauiPoint();
		var rect = CalculateElementRect(touchableView);
		var direction = ComputeDirection(dX, dY);

		if (HandlesSwipe)
		{
			var velocities = args.Velocities.Linear;
			const double velocityThreshold = 0.5;

			if (Math.Abs(velocities.X) > velocityThreshold || Math.Abs(velocities.Y) > velocityThreshold)
			{
				var swipeArgs = new SwipeEventArgs([touch], distance, new((float)velocities.X, (float)velocities.Y), rect, direction);
				SwipeFire(swipeArgs);
				goto END;
			}
		}

		var arg = new PanEventArgs([touch], distance, rect, direction, GestureStatus.Completed);
		PanFire(arg);

		END:
		isScrolling = false;
	}

	void OnManipulationUpdated(WGestureRecognizer sender, ManipulationUpdatedEventArgs args)
	{
		var dX = args.Delta.Translation.X;
		var dY = args.Delta.Translation.Y;

		var distance = args.Delta.ToMauiDistance();
		var touch = args.Position.ToMauiPoint();
		var rect = CalculateElementRect(touchableView);
		var direction = ComputeDirection(dX, dY);

		var arg = new PanEventArgs([touch], distance, rect, direction, GestureStatus.Running);
		PanFire(arg);
	}

	void OnManipulationStarted(WGestureRecognizer sender, ManipulationStartedEventArgs args)
	{
		isScrolling = true;
		var touch = args.Position.ToMauiPoint();
		var rect = CalculateElementRect(touchableView);

		var arg = new PanEventArgs([touch], Vector2.Zero, rect, Direction.Unknown, GestureStatus.Started);
		PanFire(arg);
	}

	void OnRightTapped(WGestureRecognizer sender, RightTappedEventArgs args)
	{
		HandleLongPress(touchableView, args.Position);
	}

	void OnHolding(WGestureRecognizer sender, HoldingEventArgs args)
	{
		if (args.HoldingState != HoldingState.Started)
		{
			return;
		}

		HandleLongPress(touchableView, args.Position);
	}



	void OnDoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
	{
		doubleTapCompletionSource.SetResult();
		doubleTapCompletionSource = new();
		isDoubleTap = true;

		var uiElement = (FrameworkElement?)sender ?? touchableView;

		if (uiElement is null)
		{
			return;
		}

		var touch = e.GetPosition(uiElement);
		var rect = CalculateElementRect(uiElement);
		var args = new TapEventArgs(new(touch.X, touch.Y), rect);
		DoubleTapFire(args);
		e.Handled = true;

		//TODO: FlowGesture to inner views
	}

	void OnTapped(WGestureRecognizer sender, Microsoft.UI.Input.TappedEventArgs args)
	{
		if (isDoubleTap)
		{
			isDoubleTap = false;
			return;
		}
		cts.Dispose();
		cts = RegisterNewCts();

		var touch = args.Position.ToMauiPoint();
		var rect = CalculateElementRect(touchableView);

		var arg = new TapEventArgs(touch, rect);

		Task.Run(async () => await doubleTapCompletionSource.Task.WaitAsync(cts.Token)).ContinueWith(t =>
		{
			Assert(MainThread.IsMainThread, "It should run on UIThread");

			if (t.Status == TaskStatus.Canceled)
			{
				TapFire(arg);
			}
		}, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
	}

	Rect CalculateElementRect(FrameworkElement? uiElement)
	{
		if (uiElement is null)
		{
			return Rect.Zero;
		}

		var transform = uiElement.TransformToVisual(Window.Content);
		var position = transform.TransformPoint(new Windows.Foundation.Point(0, 0));
		return new(position.X, position.Y, uiElement.ActualWidth, uiElement.ActualHeight);
	}

	void HandleLongPress(FrameworkElement uiElement, Windows.Foundation.Point position)
	{
		var rect = CalculateElementRect(uiElement);
		var touch = position.ToMauiPoint();
		var arg = new LongPressEventArgs(touch, rect);
		LongPressFire(arg);
	}

	static Direction ComputeDirection(double dX, double dY)
	{
		Direction direction;

		if (Math.Abs(dX) > Math.Abs(dY))
		{
			direction = dX > 0 ? Direction.Right : Direction.Left;
		}
		else
		{
			direction = dY > 0 ? Direction.Down : Direction.Up;
		}
		return direction;
	}
}
