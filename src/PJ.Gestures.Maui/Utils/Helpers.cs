namespace PJ.Gestures.Maui;

static partial class Helpers
{
	public static T? GetTargetOrDefault<T>(this WeakReference<T> weak) where T : class
		=> weak.TryGetTarget(out var target) ? target : null;

	public static IEnumerable<GestureBehavior> HandleGestureOnParentes(this VisualElement visualElement)
	{
		var element = visualElement.Parent;

		while (element is VisualElement parent)
		{
			foreach (var behavior in parent.Behaviors.OfType<GestureBehavior>())
			{
				yield return behavior;
			}

			element = element.Parent;
		}
	}
}
