# PJ.Gestures.Maui

A comprehensive gesture library for .NET MAUI that provides enhanced gesture support beyond the default framework capabilities. This library offers a unified way to handle tap, double tap, long press, pan, and swipe gestures with cross-platform consistency.

## Features

- **Multiple Gesture Types**: Tap, Double Tap, Long Press, Pan, and Swipe gestures
- **Cross-Platform**: Works on Android, iOS, macOS, and Windows
- **Parent Gesture Propagation**: Gestures can be received from parent elements
- **Flexible Event Handling**: Rich event args with position, direction, and status information
- **Collection View Support**: Enhanced gesture handling for scrollable content
- **Easy Integration**: Simple behavior-based implementation

## Installation

Add the NuGet package to your .NET MAUI project:

```xml
<PackageReference Include="PJ.Gestures.Maui" Version="x.x.x" />
```

## Setup

Just install the nuget and use it!

## Basic Usage

### 1. Adding Gestures to a View

Add the `GestureBehavior` to any `VisualElement` in XAML:

```xml
<ContentPage xmlns:gesture="clr-namespace:PJ.Gestures.Maui;assembly=PJ.Gestures.Maui">
    <StackLayout>
        <StackLayout.Behaviors>
            <gesture:GestureBehavior
                Tap="OnTap"
                DoubleTap="OnDoubleTap"
                LongPress="OnLongPress"
                Pan="OnPan"
                Swipe="OnSwipe" />
        </StackLayout.Behaviors>
        
        <Label Text="Touch me!" />
    </StackLayout>
</ContentPage>
```

### 2. Handling Gesture Events

In your code-behind, implement the event handlers:

```csharp
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnTap(object sender, TapEventArgs e)
    {
        // Handle single tap
        var touchPoint = e.Touch; // Point where tap occurred
        var viewBounds = e.ViewPosition; // Bounds of the view
    }

    private void OnDoubleTap(object sender, TapEventArgs e)
    {
        // Handle double tap
        var touchPoint = e.Touch;
    }

    private void OnLongPress(object sender, LongPressEventArgs e)
    {
        // Handle long press
        var touchPoint = e.Touch;
    }

    private void OnPan(object sender, PanEventArgs e)
    {
        // Handle pan gesture
        var direction = e.Direction; // Up, Down, Left, Right
        var status = e.GestureStatus; // Started, Running, Completed, Canceled
        var touches = e.Touches; // Array of touch points
        var distance = e.Distance; // Vector2 distance moved
        var center = e.Center; // Center point of touches
    }

    private void OnSwipe(object sender, SwipeEventArgs e)
    {
        // Handle swipe gesture
        var direction = e.Direction; // Up, Down, Left, Right
        var velocity = e.Velocity; // Vector2 velocity
        var distance = e.Distance; // Vector2 distance
        var touches = e.Touches; // Array of touch points
    }
}
```

## Advanced Usage

### 3. Parent Gesture Propagation

Enable gesture propagation from parent elements:

```xml
<ContentPage>
    <ContentPage.Behaviors>
        <gesture:GestureBehavior 
            ReceiveGestureFromParent="true" 
            Swipe="OnPageSwipe" />
    </ContentPage.Behaviors>
    
    <CollectionView ItemsSource="{Binding Items}">
        <!-- Collection view content -->
    </CollectionView>
</ContentPage>
```

```csharp
private void OnPageSwipe(object sender, SwipeEventArgs e)
{
    if (e.Direction == Direction.Down)
    {
        // Navigate back when swiping down
        await Navigation.PopAsync();
    }
}
```

### 4. Interactive Element Movement

Create draggable elements using pan gestures:

```xml
<AbsoluteLayout x:Name="container">
    <AbsoluteLayout.Behaviors>
        <gesture:GestureBehavior Pan="OnPan" />
    </AbsoluteLayout.Behaviors>
    
    <ContentView 
        x:Name="draggableView"
        BackgroundColor="Red"
        WidthRequest="80"
        HeightRequest="80" />
</AbsoluteLayout>
```

```csharp
private void OnPan(object sender, PanEventArgs e)
{
    var touch = e.Touches[0];
    
    switch (e.GestureStatus)
    {
        case GestureStatus.Started:
            // Initialize drag
            break;
            
        case GestureStatus.Running:
            // Update position during drag
            var x = touch.X - (draggableView.Width / 2);
            var y = touch.Y - (draggableView.Height / 2);
            AbsoluteLayout.SetLayoutBounds(draggableView, new Rect(x, y, 80, 80));
            break;
            
        case GestureStatus.Completed:
            // Finalize drag
            break;
    }
}
```

### 5. Custom Swipe Navigation Control

Create a custom control with swipe navigation:

```csharp
public class SwipeNavigationControl : ContentView
{
    private View firstPage, secondPage;
    private AbsoluteLayout mainLayout;
    private bool isOnSecondPage = false;

    public SwipeNavigationControl(View firstPage, View secondPage)
    {
        this.firstPage = firstPage;
        this.secondPage = secondPage;

        var gestureBehavior = new GestureBehavior();
        gestureBehavior.Swipe += OnSwipe;

        mainLayout = new AbsoluteLayout
        {
            Behaviors = { gestureBehavior }
        };

        SetupPages();
        Content = mainLayout;
    }

    private void SetupPages()
    {
        // Position first page
        AbsoluteLayout.SetLayoutBounds(firstPage, new Rect(0, 0, 1, 1));
        AbsoluteLayout.SetLayoutFlags(firstPage, AbsoluteLayoutFlags.All);
        mainLayout.Children.Add(firstPage);

        // Position second page off-screen
        AbsoluteLayout.SetLayoutBounds(secondPage, new Rect(0, 1, 1, 1));
        AbsoluteLayout.SetLayoutFlags(secondPage, AbsoluteLayoutFlags.All);
        mainLayout.Children.Add(secondPage);
        secondPage.TranslationY = Height;
    }

    private async void OnSwipe(object sender, SwipeEventArgs e)
    {
        switch (e.Direction)
        {
            case Direction.Up when !isOnSecondPage:
                await AnimateToSecondPage();
                break;
            case Direction.Down when isOnSecondPage:
                await AnimateToFirstPage();
                break;
        }
    }

    private async Task AnimateToSecondPage()
    {
        var animation = new Animation
        {
            { 0, 1, new Animation(v => firstPage.TranslationY = v, 0, -Height) },
            { 0, 1, new Animation(v => secondPage.TranslationY = v, Height, 0) }
        };

        animation.Commit(this, "ToSecondPage", 16, 300, Easing.Linear);
        isOnSecondPage = true;
    }

    private async Task AnimateToFirstPage()
    {
        var animation = new Animation
        {
            { 0, 1, new Animation(v => firstPage.TranslationY = v, -Height, 0) },
            { 0, 1, new Animation(v => secondPage.TranslationY = v, 0, Height) }
        };

        animation.Commit(this, "ToFirstPage", 16, 300, Easing.Linear);
        isOnSecondPage = false;
    }
}
```

## Event Arguments Reference

### TapEventArgs / DoubleTapEventArgs / LongPressEventArgs
- `Touch`: Point where the gesture occurred
- `ViewPosition`: Bounds of the view that received the gesture

### PanEventArgs
- `Touches`: Array of touch points
- `Direction`: Direction of the pan (Up, Down, Left, Right)
- `Distance`: Vector2 representing the distance moved
- `GestureStatus`: Current status (Started, Running, Completed, Canceled)
- `Center`: Center point of all touches
- `ViewPosition`: Bounds of the view

### SwipeEventArgs
- `Touches`: Array of touch points
- `Direction`: Direction of the swipe (Up, Down, Left, Right)
- `Distance`: Vector2 representing the distance of the swipe
- `Velocity`: Vector2 representing the velocity of the swipe
- `Center`: Center point of all touches
- `ViewPosition`: Bounds of the view

## Configuration

### Swipe Velocity Threshold
Adjust the sensitivity of swipe detection:

```csharp
GestureBehavior.SwipeVelocityThreshold = 0.2f; // Default is 0.1f
```

### Receive Gesture From Parent
Enable gesture propagation from parent elements:

```xml
<gesture:GestureBehavior ReceiveGestureFromParent="true" />
```

## Collection View Integration

For enhanced gesture handling with Collection Views, register the custom handler:

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureMauiHandlers(handlers =>
            {
#if !WINDOWS
                handlers.AddHandler(typeof(CollectionView), typeof(GestureCollectionViewHandler));
#endif
            })
            .UsePJGestures();

        return builder.Build();
    }
}
```

## Platform Support

- ✅ Android
- ✅ iOS
- ✅ macOS (Mac Catalyst)
- ✅ Windows

## Common Use Cases

1. **Image Galleries**: Implement pinch-to-zoom and swipe navigation
2. **Navigation**: Use swipe gestures for page transitions
3. **Interactive Games**: Handle pan gestures for drag-and-drop
4. **Context Menus**: Use long press to show context options
5. **Quick Actions**: Implement double tap for quick actions

## Support

This project is open source and maintained by one person. If you need urgent fixes or custom features, you can support the development through [GitHub Sponsors](https://github.com/sponsors/pictos/sponsorships?sponsor=pictos&tier_id=485056&preview=false).
