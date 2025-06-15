namespace PJ.Gestures.Maui.Samples;

sealed class VerticalSwipeControl : ContentView
{
	View firstPage, secondPage;
	AbsoluteLayout mainLayout;
	bool flagThatIDontKnowHowToNameIt = false;
	const uint length = 300, rate = 16;

	Action<double> animateTransitionFirstPageAction;
	Action<double> animateTransitionSecondPageAction;

	public VerticalSwipeControl(View firstPage, View secondPage)
	{
		ArgumentNullException.ThrowIfNull(firstPage);
		ArgumentNullException.ThrowIfNull(secondPage);

		this.firstPage = firstPage;
		this.secondPage = secondPage;

		animateTransitionFirstPageAction = UpdateFirstPageTranslation;
		animateTransitionSecondPageAction = UpdateSecondPageTranslation;

		var gestureB = new GestureBehavior();
		gestureB.Swipe += GestureB_Swipe;

		mainLayout = new()
		{
			Behaviors = { gestureB },
		};

		Content = mainLayout;

		this.Loaded += OnLoaded;
		this.Unloaded += (s, e) => Window.SizeChanged -= Window_SizeChanged;
	}

	void OnLoaded(object? sender, EventArgs e)
	{
		Window.SizeChanged += Window_SizeChanged;
		SetPages();
	}

	void SetPages()
	{
		AbsoluteLayout.SetLayoutBounds(secondPage, new(0, 1, 1, 1));
		AbsoluteLayout.SetLayoutFlags(secondPage, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.All);
		mainLayout.Children.Add(secondPage);
		secondPage.TranslationY = Window.Height;

		AbsoluteLayout.SetLayoutBounds(firstPage, new(0, 0, 1, 1));
		AbsoluteLayout.SetLayoutFlags(firstPage, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.All);
		mainLayout.Children.Add(firstPage);
	}

	void Window_SizeChanged(object? sender, EventArgs e)
	{
		if (sender is not Window window)
		{
			return;
		}

		if (flagThatIDontKnowHowToNameIt)
		{
			firstPage.TranslationY = window.Height;
		}
		else
		{
			firstPage.TranslationY = window.Height;
		}
	}

	void GestureB_Swipe(object? sender, SwipeEventArgs e)
	{
		switch (e.Direction)
		{
			case Direction.Up:
				AnimateTransitionUp();
				break;
			case Direction.Down:
				AnimateTransitionDown();
				break;
		}
	}

	void AnimateTransitionUp()
	{
		var height = Window.Height;
		if (flagThatIDontKnowHowToNameIt)
			return;


		var firstPageAnimation = new Animation(animateTransitionFirstPageAction, 0, -height);
		var secondPageAnimation = new Animation(animateTransitionSecondPageAction, height, 0);

		var animation = new Animation
		{
			{ 0, 1, firstPageAnimation },
			{ 0, 1, secondPageAnimation }
		};

		animation.Commit(this, "PageSwapUp", rate, length, Easing.Linear, (v, c) => flagThatIDontKnowHowToNameIt = true, () => false);
	}

	void AnimateTransitionDown()
	{
		var height = Window.Height;
		if (!flagThatIDontKnowHowToNameIt)
			return;

		var firstPageAnimation = new Animation(animateTransitionFirstPageAction, -height, 0);
		var secondPageAnimation = new Animation(animateTransitionSecondPageAction, 0, height);

		var animation = new Animation
		{
			{ 0, 1, firstPageAnimation },
			{ 0, 1, secondPageAnimation }
		};

		animation.Commit(this, "PageSwapDown", rate, length, Easing.Linear, (v, c) => flagThatIDontKnowHowToNameIt = false, () => false);
	}

	void UpdateFirstPageTranslation(double v)
	{
		firstPage.TranslationY = v;
	}

	void UpdateSecondPageTranslation(double v)
	{
		secondPage.TranslationY = v;
	}
}