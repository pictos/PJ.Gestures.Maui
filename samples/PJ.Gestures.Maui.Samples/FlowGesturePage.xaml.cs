namespace PJ.Gestures.Maui.Samples;

public partial class FlowGesturePage : ContentPage
{
	public FlowGesturePage()
	{
		InitializeComponent();

		var list = new List<string>(100);
		for(var i = 0; i < 100; i++)
		{
			list.Add($"Item {i}");
		}

		cv.ItemsSource = list;
	}

	void GestureBehavior_Swipe(object sender, SwipeEventArgs e)
	{
		switch(e.Direction)
		{
			case Direction.Down:
				Navigation.PopAsync();
				break;
		}
	}
}