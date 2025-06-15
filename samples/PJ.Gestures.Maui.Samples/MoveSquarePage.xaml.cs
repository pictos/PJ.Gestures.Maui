namespace PJ.Gestures.Maui.Samples;

public partial class MoveSquarePage : ContentPage
{
	public MoveSquarePage()
	{
		InitializeComponent();
	}

	double touchY;
	static readonly Size autoSize = new(AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
	void GestureBehavior_Pan(object sender, PanEventArgs e)
	{
		Log($"Pan Direction: {e.Direction}");

		var touch = e.Touches[0];

		double x, y = 0;

		switch (e.GestureStatus)
		{
			case GestureStatus.Started:
				this.lbl.Text = $"Pan {e.Direction}, Started";
				x = touch.X - (this.contentView.Width / 2);
				touchY = y = touch.Y - (this.contentView.Height / 2);
				abs.SetLayoutBounds(contentView, new(new(x, y), autoSize));
				break;
			case GestureStatus.Running:

				x = touch.X - (this.contentView.Width / 2);
				y = touch.Y - (this.contentView.Height / 2);
				abs.SetLayoutBounds(contentView, new(new(x, y), autoSize));
				this.lbl.Text = $"Pan {e.Direction}, Running";
				break;
			case GestureStatus.Completed:
				this.lbl.Text = $"Pan {e.Direction}, Completed";
				break;
			case GestureStatus.Canceled:
				this.lbl.Text = $"Pan {e.Direction}, Canceled";
				break;
		}
	}


	static void Log(string txt)
	{
		WriteLine("#############");
		WriteLine(txt);
		WriteLine("#############");
	}
}