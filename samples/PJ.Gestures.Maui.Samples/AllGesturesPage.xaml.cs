namespace PJ.Gestures.Maui.Samples;

public partial class AllGesturesPage : ContentPage
{
	public AllGesturesPage()
	{
		InitializeComponent();
	}

	void Log(string txt)
	{
		lbl.Text = txt;
	}

	void GestureBehavior_DoubleTap(object sender, TapEventArgs e)
	{
		Log("DoubleTap");

		var g = (GestureBehavior)sender;
		
	}

	void GestureBehavior_Tap(object sender, TapEventArgs e)
	{
		Log("SingleTap");
	}

	void GestureBehavior_Pan(object sender, PanEventArgs e)
	{
		Log($"Pan {e.Direction}, and {e.GestureStatus}");
	}

	void GestureBehavior_Swipe(object sender, SwipeEventArgs e)
	{
		Log($"Swipe: {e.Direction}");
	}

	void GestureBehavior_LongPress(object sender, LongPressEventArgs e)
	{
		Log("LongPress");
	}
}