using System;
using System.Linq;

namespace PJ.Gestures.Maui.Samples;
static class MauiExtensions
{
	public static IEnumerable<GestureBehavior> HandleGestureOnParents(this VisualElement visualElement)
	{
		var p = visualElement.Parent;
		while (p is VisualElement parent)
		{
			foreach (var b in parent.Behaviors.OfType<GestureBehavior>())
				yield return b;

			p = p.Parent;
		}
	}
}