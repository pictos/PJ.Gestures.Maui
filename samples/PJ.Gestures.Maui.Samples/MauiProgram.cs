using Microsoft.Extensions.Logging;
#if !WINDOWS
using PJ.Gestures.Maui.Samples.Controls;
#endif

namespace PJ.Gestures.Maui.Samples;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.ConfigureMauiHandlers(h =>
			{
#if !WINDOWS
				h.AddHandler(typeof(CollectionView), typeof(GestureCollectionViewHandler));
#endif
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}