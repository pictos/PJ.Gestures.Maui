using System.Buffers;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Platform;

namespace PJ.Gestures.Maui;
partial class Helpers
{
	public static Rect GetViewPosition(this AView? view)
	{
		if (view is null)
			return Rect.Zero;

		var location = ArrayPool<int>.Shared.Rent(2);

		view.GetLocationInWindow(location);

		var x = location[0];
		var y = location[1];
		var width = view.Width;
		var height = view.Height;

		ArrayPool<int>.Shared.Return(location);

		return DIP.ToRect(x, y, width, height);
	}

	public static Vector2 CalculateDistances(MotionEvent? e1, MotionEvent? e2, Context context)
	{
		if (e1 is null & e2 is null)
		{
			return Vector2.Zero;
		}

		if (e1 is null && e2 is not null)
		{
			return ComputeVector(e2.GetX(), e2.GetY(), context);
		}

		if (e1 is not null && e2 is null)
		{
			return ComputeVector(e1.GetX(), e1.GetY(), context);
		}

		Assert(e2 is not null);
		Assert(e1 is not null);

		var dX = e2.GetX() - e1.GetX();
		var dY = e2.GetY() - e2.GetY();

		return ComputeVector(dX, dY, context);
		
		static Vector2 ComputeVector(float x, float y, Context context)
		{
			x = (float)context.FromPixels(x);
			y = (float)context.FromPixels(y);

			return new(x, y);
		}
	}

	public static GestureStatus ToGestureStatus(this MotionEventActions actions) => actions switch
	{
		MotionEventActions.ButtonPress => GestureStatus.Started,
		MotionEventActions.ButtonRelease => GestureStatus.Completed,
		MotionEventActions.Cancel => GestureStatus.Canceled,
		MotionEventActions.Down => GestureStatus.Started,
		MotionEventActions.Move => GestureStatus.Running,
		MotionEventActions.Outside => GestureStatus.Running,
		MotionEventActions.Pointer1Down => GestureStatus.Started,
		MotionEventActions.Pointer1Up => GestureStatus.Completed,
		MotionEventActions.Pointer2Down => GestureStatus.Started,
		MotionEventActions.Pointer2Up => GestureStatus.Completed,
		MotionEventActions.Pointer3Down => GestureStatus.Started,
		MotionEventActions.Pointer3Up => GestureStatus.Completed,
		MotionEventActions.Up => GestureStatus.Completed,
		_ => GestureStatus.Canceled
	};
}

static class DIP
{
	internal static readonly double Density = DeviceDisplay.MainDisplayInfo.Density;

	internal static Point ToPoint(double dipX, double dipY)
	{
		return new Point(dipX / Density, dipY / Density);
	}

	internal static Rect ToRect(double dipX, double dipY, double dipWidth, double dipHeight)
	{
		return new Rect(dipX / Density, dipY / Density, dipWidth / Density, dipHeight / Density);
	}
}
