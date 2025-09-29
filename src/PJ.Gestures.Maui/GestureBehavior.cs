using System.Runtime.CompilerServices;

namespace PJ.Gestures.Maui;

/// <summary>
/// Provides gesture handling behavior for a <see cref="VisualElement"/> in .NET MAUI.
/// Supports tap, double tap, long press, pan, and swipe gestures.
/// </summary>
public partial class GestureBehavior : PlatformBehavior<VisualElement>
{
	VisualElement view = default!;
	TaskCompletionSource doubleTapCompletionSource = new();
	CancellationTokenSource cts = RegisterNewCts();

	/// <summary>
	/// Occurs when a tap gesture is detected.
	/// </summary>
	public event EventHandler<TapEventArgs>? Tap;

	/// <summary>
	/// Occurs when a double tap gesture is detected.
	/// </summary>
	public event EventHandler<TapEventArgs>? DoubleTap;

	/// <summary>
	/// Occurs when a long press gesture is detected.
	/// </summary>
	public event EventHandler<LongPressEventArgs>? LongPress;

	/// <summary>
	/// Occurs when a pan gesture is detected.
	/// </summary>
	public event EventHandler<PanEventArgs>? Pan;

	/// <summary>
	/// Occurs when a swipe gesture is detected.
	/// </summary>
	public event EventHandler<SwipeEventArgs>? Swipe;

	bool HandlesSwipe
	{
		get
		{
			var swipes = Swipe?.GetInvocationList() ?? [];
			return swipes.Length > 0;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this behavior should receive gestures from its parent.
	/// </summary>
	public bool ReceiveGestureFromParent { get; set; }

	public bool FlowGesture { get; set; }

	/// <summary>
	/// Gets or sets the velocity threshold for swipe detection.
	/// </summary>
	public static float SwipeVelocityThreshold { get; set; } = 0.1F;

	/// <summary>
	/// Invokes the <see cref="Tap"/> event.
	/// </summary>
	/// <param name="args">The tap event arguments.</param>
	internal void TapFire(TapEventArgs args) => Tap?.Invoke(view, args);

	/// <summary>
	/// Invokes the <see cref="LongPress"/> event.
	/// </summary>
	/// <param name="args">The long press event arguments.</param>
	internal void LongPressFire(LongPressEventArgs args) => LongPress?.Invoke(view, args);

	/// <summary>
	/// Invokes the <see cref="Pan"/> event.
	/// </summary>
	/// <param name="args">The pan event arguments.</param>
	internal void PanFire(PanEventArgs args) => Pan?.Invoke(view, args);

	/// <summary>
	/// Invokes the <see cref="Swipe"/> event.
	/// </summary>
	/// <param name="args">The swipe event arguments.</param>
	internal void SwipeFire(SwipeEventArgs args) => Swipe?.Invoke(view, args);

	/// <summary>
	/// Invokes the <see cref="DoubleTap"/> event.
	/// </summary>
	/// <param name="args">The double tap event arguments.</param>
	internal void DoubleTapFire(TapEventArgs args) => DoubleTap?.Invoke(view, args);

#if DEBUG
	static void Logger([CallerMemberName] string name = "", [CallerLineNumber] int number = 0)
	{
		System.Diagnostics.Debug.WriteLine(" ##############################");
		System.Diagnostics.Debug.WriteLine($"Called from {name} at line: {number}");
	}
#endif

	/// <summary>
	/// Handles a gesture event received from the parent element, if <see cref="ReceiveGestureFromParent"/> is true.
	/// </summary>
	/// <param name="args">The gesture event arguments.</param>
	/// <exception cref="InvalidOperationException">Thrown if the event type is not supported.</exception>
	public void FlowGestureImpl(BaseEventArgs args)
	{
		switch (args)
		{
			case TapEventArgs tap:
				TapFire(tap);
				break;

			case LongPressEventArgs longPress:
				LongPressFire(longPress);
				break;
			case SwipeEventArgs swipe:
				SwipeFire(swipe);
				break;
			case PanEventArgs pan:
				PanFire(pan);
				break;
			default:
				throw new InvalidOperationException($"There's no case for {args.GetType().FullName}.");
		}
	}

	public void SendGestureToParent(BaseEventArgs args)
	{
		if (!FlowGesture)
		{
			return;
		}

		foreach (var behavior in view.HandleGestureOnParents())
		{
			behavior.FlowGestureImpl(args);
		}

	}

	/// <summary>
	/// Registers a new <see cref="CancellationTokenSource"/> with a 100ms timeout.
	/// </summary>
	/// <returns>A new <see cref="CancellationTokenSource"/> instance.</returns>
	internal static CancellationTokenSource RegisterNewCts() =>
		new(TimeSpan.FromMilliseconds(100));
}
