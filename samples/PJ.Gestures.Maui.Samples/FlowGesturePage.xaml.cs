namespace PJ.Gestures.Maui.Samples;

public partial class FlowGesturePage : ContentPage
{
	public FlowGesturePage()
	{
		InitializeComponent();

	}

	void GestureBehavior_Swipe(object sender, SwipeEventArgs e)
	{
		switch (e.Direction)
		{
			case Direction.Down:
				Navigation.PopAsync();
				break;
		}
	}

	void GestureBehavior_Tap(object sender, TapEventArgs e)
	{

	}

	void GestureBehavior_Tap_1(object sender, TapEventArgs e)
	{

	}
}