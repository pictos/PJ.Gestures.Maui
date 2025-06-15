namespace PJ.Gestures.Maui.Samples;
sealed class VerticalSwipePage : ContentPage
{
	public VerticalSwipePage()
	{
		var c1 = new ContentView
		{
			Content = new Label { Text = "C1", VerticalTextAlignment = TextAlignment.Center, Background = Colors.Fuchsia }
		};

		var c2 = new ContentView
		{
			Content = new Label { Text = "C2", VerticalTextAlignment = TextAlignment.Center, Background = Colors.Orange }
		};

		Content = new VerticalSwipeControl(c1, c2);
	}
}
