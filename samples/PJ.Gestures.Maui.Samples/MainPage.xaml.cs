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
}
