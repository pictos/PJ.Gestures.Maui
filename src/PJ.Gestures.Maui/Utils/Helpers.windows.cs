using Microsoft.UI.Input;
using WPoint = Windows.Foundation.Point;

namespace PJ.Gestures.Maui.Utils;

static class Helpers
{
	public static Point ToMauiPoint(this WPoint wPoint) =>
		new(wPoint.X, wPoint.Y);

	public static Vector2 ToMauiDistance(this ManipulationDelta delta) =>
		new((float)delta.Translation.X, (float)delta.Translation.Y);
}