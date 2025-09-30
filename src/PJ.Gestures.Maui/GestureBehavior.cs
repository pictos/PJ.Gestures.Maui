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
	/// Gets or sets a value indicating whether this behavior should receive and process
	/// gesture events that were originated on descendant <see cref="VisualElement"/>s
	/// and bubbled upward by those descendants via <see cref="FlowGesture"/>.
	/// </summary>
	/// <remarks>
	/// For a parent behavior to receive a child's gesture:
	/// 1. The child behavior must have <see cref="FlowGesture"/> set to <c>true</c>.
	/// 2. The parent behavior must have <see cref="ReceiveGestureFromChild"/> set to <c>true</c>.
	/// 
	/// When <c>false</c> (default), this behavior only reacts to gestures performed directly
	/// on its associated view and ignores bubbled gestures from descendants.
	/// </remarks>
	/// <value>
	/// <c>true</c> to allow handling of gestures bubbled from child elements; otherwise, <c>false</c>.
	/// </value>
	public bool ReceiveGestureFromChild { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether gesture events handled by this behavior
	/// should be propagated to ancestor <see cref="VisualElement"/> gesture behaviors.
	/// </summary>
	/// <remarks>
	/// When <c>true</c>, each recognized gesture (tap, double tap, long press, pan, swipe)
	/// is forwarded via <see cref="SendGestureToParent(BaseEventArgs)"/> so parent behaviors
	/// can also process it. Use this to enable layered or cooperative gesture handling.
	/// When <c>false</c> (default), gestures are not bubbled upward.
	/// </remarks>
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

	void FlowGestureImpl(BaseEventArgs args)
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

	/// <summary>
	/// Handles a gesture event received from the parent element, if <see cref="ReceiveGestureFromChild"/> is true.
	/// </summary>
	/// <param name="args">The gesture event arguments.</param>
	/// <exception cref="InvalidOperationException">Thrown if the event type is not supported.</exception>
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
