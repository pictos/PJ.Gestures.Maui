namespace PJ.Gestures.Maui;

static partial class Helpers
{
	/// <summary>
	/// Retrieves the target object from a weak reference, or returns null if the target is no longer available.
	/// </summary>
	/// <typeparam name="T">The type of the target object.</typeparam>
	/// <param name="weak">The weak reference containing the target object.</param>
	/// <returns>The target object if it is available; otherwise, null.</returns>
	public static T? GetTargetOrDefault<T>(this WeakReference<T> weak) where T : class
		=> weak.TryGetTarget(out var target) ? target : null;

	public static IEnumerable<GestureBehavior> HandleGestureOnParents(this VisualElement visualElement)
	{
		var element = visualElement.Parent;

		while (element is VisualElement parent)
		{
			foreach (var behavior in parent.Behaviors.OfType<GestureBehavior>().Where(x => x.ReceiveGestureFromParent))
			{
				yield return behavior;
			}

			element = element.Parent;
		}
	}
}
