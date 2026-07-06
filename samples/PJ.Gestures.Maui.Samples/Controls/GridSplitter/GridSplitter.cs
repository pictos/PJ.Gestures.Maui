using PJ.Gestures.Maui;
using System.Runtime.CompilerServices;

namespace PJ.Gestures.Maui.Samples.Controls.GridSplitter;

public sealed partial class GridSplitter : TemplatedView
{
	const string ElementGridSplitter = "PART_GridSplitter";

	Grid? gridSplitter;
	readonly GestureBehavior gestureBehavior = new();

	public static readonly BindableProperty ElementProperty =
		BindableProperty.Create(nameof(Element), typeof(View), typeof(GridSplitter), null);

	public View Element
	{
		get => (View)GetValue(ElementProperty);
		set => SetValue(ElementProperty, value);
	}

	public static readonly BindableProperty ResizeDirectionProperty =
		BindableProperty.Create(nameof(ResizeDirection), typeof(GridResizeDirection), typeof(GridSplitter), GridResizeDirection.Auto);

	public GridResizeDirection ResizeDirection
	{
		get => (GridResizeDirection)GetValue(ResizeDirectionProperty);
		set => SetValue(ResizeDirectionProperty, value);
	}

	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		gridSplitter = (Grid)GetTemplateChild(ElementGridSplitter);

		Assert(gridSplitter is not null);

		UpdateIsEnabled();
	}

	protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		base.OnPropertyChanged(propertyName);

		if (propertyName == nameof(ResizeDirection))
		{
			UpdateLayout();
		}
		else if (propertyName == nameof(IsEnabled))
		{
			UpdateIsEnabled();
		}
	}

	void UpdateIsEnabled()
	{
		if (gridSplitter is null)
		{
			return;
		}

		if (IsEnabled)
		{
			gestureBehavior.Pan -= OnPanUpdated;
			gestureBehavior.Pan += OnPanUpdated;

			if (!gridSplitter.Behaviors.Contains(gestureBehavior))
			{
				gridSplitter.Behaviors.Add(gestureBehavior);
			}
		}
		else
		{
			gestureBehavior.Pan -= OnPanUpdated;
			gridSplitter.Behaviors.Remove(gestureBehavior);
		}
	}

	void OnPanUpdated(object? sender, PanEventArgs e)
	{
		if (e.GestureStatus == GestureStatus.Running)
		{
			UpdateLayout(e.Distance.X, e.Distance.Y);
		}
	}

	void UpdateLayout(double offsetX = 0, double offsetY = 0)
	{
		if (Parent is not Grid)
		{
			return;
		}

		WriteLine($"OffsetX: {offsetX}, OffsetY: {offsetY}");
		if (ResizeDirection == GridResizeDirection.Columns)
		{
			UpdateColumns(offsetX);
		}
		else
		{
			UpdateRows(offsetY);
		}
	}

	void UpdateRows(double offsetY)
	{
		if (offsetY is 0 || Parent is not Grid grid)
		{
			return;
		}

		var row = Grid.GetRow(this);
		var rowCount = grid.RowDefinitions.Count;

		if (rowCount <= 1 || row is 0 || row >= rowCount - 1)
		{
			return;
		}

		var topRow = grid.RowDefinitions[row - 1];
		var bottomRow = grid.RowDefinitions[row + 1];

		var topRowHeight = topRow.Height.IsAbsolute
			? topRow.Height.Value
			: GetRowHeightBefore(grid, row);

		var bottomRowHeight = bottomRow.Height.IsAbsolute
			? bottomRow.Height.Value
			: GetAdjacentRowHeight(grid, row);

		if (topRowHeight <= 0 || bottomRowHeight <= 0)
		{
			return;
		}

		var newTopHeight = topRowHeight + offsetY;
		var newBottomHeight = bottomRowHeight - offsetY;

		if (newTopHeight < 0 || newBottomHeight < 0)
		{
			return;
		}

		NormalizeStarRows(grid, row - 1, row + 1);

		topRow.Height = new(newTopHeight, GridUnitType.Star);
		bottomRow.Height = new(newBottomHeight, GridUnitType.Star);
	}

	static void NormalizeStarRows(Grid grid, int skipFirst, int skipSecond)
	{
		for (var i = 0; i < grid.RowDefinitions.Count; i++)
		{
			if (i == skipFirst || i == skipSecond)
			{
				continue;
			}

			var definition = grid.RowDefinitions[i];
			if (!definition.Height.IsStar)
			{
				continue;
			}

			var actualHeight = GetActualRowHeight(grid, i);
			if (actualHeight > 0)
			{
				definition.Height = new(actualHeight, GridUnitType.Star);
			}
		}
	}

	static double GetActualRowHeight(Grid grid, int rowIndex)
	{
		double topBoundary = 0;
		var bottomBoundary = grid.Height;

		foreach (var child in grid.Children)
		{
			if (child is not GridSplitter other)
			{
				continue;
			}

			var otherRow = Grid.GetRow(other);
			var otherBottom = other.Bounds.Y + other.Bounds.Height;
			if (otherRow < rowIndex && otherBottom > topBoundary)
			{
				topBoundary = otherBottom;
			}
			else if (otherRow > rowIndex && other.Bounds.Y < bottomBoundary)
			{
				bottomBoundary = other.Bounds.Y;
			}
		}

		return bottomBoundary - topBoundary;
	}

	void UpdateColumns(double offsetX)
	{
		if (offsetX is 0 || Parent is not Grid grid)
		{
			return;
		}

		var column = Grid.GetColumn(this);
		var columnCount = grid.ColumnDefinitions.Count;

		if (columnCount <= 1 || column is 0 || column >= columnCount - 1)
		{
			return;
		}

		var leftColumn = grid.ColumnDefinitions[column - 1];
		var rightColumn = grid.ColumnDefinitions[column + 1];

		var leftColumnWidth = leftColumn.Width.IsAbsolute
			? leftColumn.Width.Value
			: GetColumnWidthBefore(grid, column);

		var rightColumnWidth = rightColumn.Width.IsAbsolute
			? rightColumn.Width.Value
			: GetAdjacentColumnWidth(grid, column);

		if (leftColumnWidth <= 0 || rightColumnWidth <= 0)
		{
			return;
		}

		var newLeftWidth = leftColumnWidth + offsetX;
		var newRightWidth = rightColumnWidth - offsetX;

		if (newLeftWidth < 0 || newRightWidth < 0)
		{
			return;
		}

		NormalizeStarColumns(grid, column - 1, column + 1);

		leftColumn.Width = new(newLeftWidth, GridUnitType.Star);
		rightColumn.Width = new(newRightWidth, GridUnitType.Star);
	}

	static void NormalizeStarColumns(Grid grid, int skipFirst, int skipSecond)
	{
		for (var i = 0; i < grid.ColumnDefinitions.Count; i++)
		{
			if (i == skipFirst || i == skipSecond)
			{
				continue;
			}

			var definition = grid.ColumnDefinitions[i];
			if (!definition.Width.IsStar)
			{
				continue;
			}

			var actualWidth = GetActualColumnWidth(grid, i);
			if (actualWidth > 0)
			{
				definition.Width = new(actualWidth, GridUnitType.Star);
			}
		}
	}

	static double GetActualColumnWidth(Grid grid, int columnIndex)
	{
		double leftBoundary = 0;
		var rightBoundary = grid.Width;

		foreach (var child in grid.Children)
		{
			if (child is not GridSplitter other)
			{
				continue;
			}

			var otherColumn = Grid.GetColumn(other);
			var otherRight = other.Bounds.X + other.Bounds.Width;
			if (otherColumn < columnIndex && otherRight > leftBoundary)
			{
				leftBoundary = otherRight;
			}
			else if (otherColumn > columnIndex && other.Bounds.X < rightBoundary)
			{
				rightBoundary = other.Bounds.X;
			}
		}

		return rightBoundary - leftBoundary;
	}

	double GetAdjacentColumnWidth(Grid grid, int splitterColumn)
	{
		var splitterRight = Bounds.X + Bounds.Width;
		double nextBoundary = grid.Width;

		foreach (var child in grid.Children)
		{
			if (child is GridSplitter other && other != this)
			{
				var otherColumn = Grid.GetColumn(other);
				if (otherColumn > splitterColumn)
				{
					nextBoundary = other.Bounds.X;
					break;
				}
			}
		}

		return nextBoundary - splitterRight;
	}

	double GetColumnWidthBefore(Grid grid, int splitterColumn)
	{
		double previousBoundary = 0;

		foreach (var child in grid.Children)
		{
			if (child is GridSplitter other && other != this)
			{
				var otherColumn = Grid.GetColumn(other);
				if (otherColumn < splitterColumn)
				{
					previousBoundary = other.Bounds.X + other.Bounds.Width;
				}
			}
		}

		return Bounds.X - previousBoundary;
	}

	double GetAdjacentRowHeight(Grid grid, int splitterRow)
	{
		var splitterBottom = Bounds.Y + Bounds.Height;
		double nextBoundary = grid.Height;

		foreach (var child in grid.Children)
		{
			if (child is GridSplitter other && other != this)
			{
				var otherRow = Grid.GetRow(other);
				if (otherRow > splitterRow)
				{
					nextBoundary = other.Bounds.Y;
					break;
				}
			}
		}

		return nextBoundary - splitterBottom;
	}

	double GetRowHeightBefore(Grid grid, int splitterRow)
	{
		double previousBoundary = 0;

		foreach (var child in grid.Children)
		{
			if (child is GridSplitter other && other != this)
			{
				var otherRow = Grid.GetRow(other);
				if (otherRow < splitterRow)
				{
					previousBoundary = other.Bounds.Y + other.Bounds.Height;
				}
			}
		}

		return Bounds.Y - previousBoundary;
	}
}
