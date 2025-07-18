namespace PJ.Gestures.Maui.Samples;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		Navigation.PushAsync(new MoveSquarePage());
	}

	void Button_Clicked_1(object sender, EventArgs e)
	{
		Navigation.PushAsync(new VerticalSwipePage());
	}

	void Button_Clicked_2(object sender, EventArgs e)
	{
		Navigation.PushAsync(new AllGesturesPage());
	}

	void Button_Clicked_3(object sender, EventArgs e)
	{
		Navigation.PushAsync(new FlowGesturePage());
	}
}
