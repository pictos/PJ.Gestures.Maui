using CoreGraphics;
using Foundation;
using UIKit;

namespace PJ.Gestures.Maui;

partial class GestureBehavior
{
	UIContextMenuInteraction? contextMenuInteraction;
	RightClickInteractionDelegate? contextMenuDelegate;

	partial void OnAttachedToPlatform(UIView platformView)
	{
		contextMenuDelegate = new RightClickInteractionDelegate(platformView, this);
		contextMenuInteraction = new UIContextMenuInteraction(contextMenuDelegate);
		platformView.AddInteraction(contextMenuInteraction);
	}

	partial void OnDetachedFromPlatform(UIView platformView)
	{
		if (contextMenuInteraction is not null)
		{
			platformView.RemoveInteraction(contextMenuInteraction);
			contextMenuInteraction = null;
			contextMenuDelegate = null;
		}
	}
}

/// <summary>
/// Intercepts secondary (right) mouse button clicks on Mac Catalyst via
/// <see cref="UIContextMenuInteraction"/> and fires the <see cref="GestureBehavior.LongPress"/>
/// event without displaying a context menu, mirroring the Windows <c>RightTapped</c> behavior.
/// </summary>
sealed class RightClickInteractionDelegate : NSObject, IUIContextMenuInteractionDelegate
{
	readonly UIView platformView;
	readonly GestureBehavior behavior;

	public RightClickInteractionDelegate(UIView platformView, GestureBehavior behavior)
	{
		this.platformView = platformView;
		this.behavior = behavior;
	}

	public UIContextMenuConfiguration? GetConfigurationForMenu(UIContextMenuInteraction interaction, CGPoint location)
	{
		var viewBounds = platformView.Bounds;
		var rect = new Rect(viewBounds.X, viewBounds.Y, viewBounds.Width, viewBounds.Height);
		var touch = new Point(location.X, location.Y);

		var args = new LongPressEventArgs(touch, rect);
		behavior.LongPressFire(args);
		behavior.SendGestureToParent(args);

		// Return null to suppress the system context menu
		return null;
	}
}
