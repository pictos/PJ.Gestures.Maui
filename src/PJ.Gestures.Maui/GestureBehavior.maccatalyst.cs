using Foundation;
using UIKit;

namespace PJ.Gestures.Maui;

partial class GestureBehavior
{
	UIGestureRecognizer? secondaryClickGestureRecognizer;

	partial void OnAttachedToPlatform(UIView platformView)
	{
		secondaryClickGestureRecognizer = new SecondaryClickGestureRecognizer(SecondaryClickHandler)
		{
			Delegate = multipleTouchesDelegate
		};
		platformView.AddGestureRecognizer(secondaryClickGestureRecognizer);
	}

	partial void OnDetachedFromPlatform(UIView platformView)
	{
		if (secondaryClickGestureRecognizer is not null)
		{
			platformView.RemoveGestureRecognizer(secondaryClickGestureRecognizer);
			secondaryClickGestureRecognizer.Delegate = default!;
		}
	}

	void SecondaryClickHandler(UIGestureRecognizer gesture)
	{
		var view = gesture.View;
		var rect = CalculateViewPosition(view);
		var touch = CalculateTouch(gesture, view);
		var args = new LongPressEventArgs(touch, rect);
		LongPressFire(args);
		SendGestureToParent(args);
	}
}

/// <summary>
/// Detects a secondary (right) mouse button click on Mac Catalyst and fires immediately on press.
/// Mimics the Windows <c>RightTapped</c> gesture behavior which also maps right-click to long press.
/// </summary>
sealed class SecondaryClickGestureRecognizer : UIGestureRecognizer
{
	readonly Action<UIGestureRecognizer> handler;

	public SecondaryClickGestureRecognizer(Action<UIGestureRecognizer> handler)
	{
		this.handler = handler;
	}

	public override void TouchesBegan(NSSet touches, UIEvent evt)
	{
		base.TouchesBegan(touches, evt);

		if (touches.AnyObject is UITouch touch &&
			touch.Type == UITouchType.IndirectPointer &&
			(evt.ButtonMask & UIEventButtonMask.Secondary) != 0)
		{
			State = UIGestureRecognizerState.Recognized;
			handler(this);
		}
		else
		{
			State = UIGestureRecognizerState.Failed;
		}
	}
}
