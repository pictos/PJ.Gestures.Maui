using System.ComponentModel;
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

	/// <summary>
	/// Forwards routed gesture / pointer events coming from a child element to this behavior so
	/// that they participate in the same gesture recognition pipeline as events raised directly
	/// on the attached (root) <see cref="FrameworkElement"/>.
	/// </summary>
	/// <param name="args">
	/// The routed event arguments originating from a child element. Supported types:
	/// <list type="bullet">
	/// <item><description><see cref="Microsoft.UI.Xaml.Input.PointerRoutedEventArgs"/>: Replayed into the pointer
	/// lifecycle handlers (<c>OnPointerPressed</c>, <c>OnPointerMoved</c>, <c>OnPointerReleased</c>, <c>OnPointerCanceled</c>).</description></item>
	/// <item><description><see cref="Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs"/>: Forwarded to
	/// <c>OnDoubleTapped</c> to preserve double-tap detection logic.</description></item>
	/// </list>
	/// </param>
	/// <remarks>
	/// <para>
	/// This method is only active when <c>ReceiveGestureFromChild</c> is true (property defined in another
	/// partial of <see cref="GestureBehavior"/>). It does NOT call the underlying <see cref="WGestureRecognizer"/>
	/// directly; instead it reuses the existing pointer handlers to maintain identical state transitions and
	/// gesture cancellation semantics.
	/// </para>
	/// <para>
	/// Pointer forwarding logic:
	/// <list type="number">
	/// <item>Determines a stable <see cref="FrameworkElement"/> (original source if possible, else the root touchable view)
	/// to keep coordinate space consistent.</item>
	/// <item>Maps <see cref="PointerUpdateKind"/> to Pressed / Released / Move flows; canceled or out-of-range
	/// transitions are converted into a cancel pathway.</item>
	/// <item>Skips processing if the pointer is no longer in range and not in contact (mirrors native cancel heuristics).</item>
	/// </list>
	/// </para>
	/// <para>
	/// Double-tap events are surfaced separately on Windows (XAML raises a distinct routed event), so they must be
	/// explicitly captured here to keep parity with tap / double-tap timing logic implemented elsewhere.
	/// </para>
	/// <para>
	/// Unsupported routed event argument types are ignored silently.
	/// </para>
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public void HandleGestureFromChild(RoutedEventArgs args)
	{
		ArgumentNullException.ThrowIfNull(args);
		if (!ReceiveGestureFromChild)
			return;

		switch (args)
		{
			case Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e:
				{
					var sourceElement = e.OriginalSource as FrameworkElement ?? touchableView;

					var point = e.GetCurrentPoint(sourceElement);
					var props = point.Properties;

					if (props.IsCanceled)
					{
						OnPointerCanceled(sourceElement, e);
						return;
					}

					if (!e.Pointer.IsInRange && !e.Pointer.IsInContact)
					{
						OnPointerCanceled(sourceElement, e);
						return;
					}

					var kind = props.PointerUpdateKind;

					if (kind is PointerUpdateKind.LeftButtonPressed
						or PointerUpdateKind.RightButtonPressed
						or PointerUpdateKind.MiddleButtonPressed
						or PointerUpdateKind.XButton1Pressed
						or PointerUpdateKind.XButton2Pressed)
					{
						OnPointerPressed(sourceElement, e);
						return;
					}

					if (kind is PointerUpdateKind.LeftButtonReleased
						or PointerUpdateKind.RightButtonReleased
						or PointerUpdateKind.MiddleButtonReleased
						or PointerUpdateKind.XButton1Released
						or PointerUpdateKind.XButton2Released)
					{
						OnPointerReleased(sourceElement, e);
						return;
					}

					OnPointerMoved(sourceElement, e);
					return;
				}

			case Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs d:
				OnDoubleTapped(d.OriginalSource, d);
				return;
		}
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
			SendGestureToParent(arg);
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
				SendGestureToParent(swipeArgs);
				goto END;
			}
		}

		var arg = new PanEventArgs([touch], distance, rect, direction, GestureStatus.Completed);
		PanFire(arg);
		SendGestureToParent(arg);

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
		SendGestureToParent(arg);
	}

	void OnManipulationStarted(WGestureRecognizer sender, ManipulationStartedEventArgs args)
	{
		isScrolling = true;
		var touch = args.Position.ToMauiPoint();
		var rect = CalculateElementRect(touchableView);

		var arg = new PanEventArgs([touch], Vector2.Zero, rect, Direction.Unknown, GestureStatus.Started);
		PanFire(arg);
		SendGestureToParent(arg);
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
		SendGestureToParent(args);

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
				SendGestureToParent(arg);
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
		SendGestureToParent(arg);
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