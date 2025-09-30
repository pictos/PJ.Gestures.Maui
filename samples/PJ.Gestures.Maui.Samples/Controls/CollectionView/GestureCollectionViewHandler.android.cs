using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Platform;

namespace PJ.Gestures.Maui.Samples.Controls;

class GestureRecyclerView : MauiRecyclerView<ReorderableItemsView, GroupableItemsViewAdapter<ReorderableItemsView, IGroupableItemsViewSource>, IGroupableItemsViewSource>
{
	bool isScrolling, isAtTop = true;

	public GestureRecyclerView(Context context, Func<IItemsLayout> getItemsLayout, Func<GroupableItemsViewAdapter<ReorderableItemsView, IGroupableItemsViewSource>> getAdapter) : base(context, getItemsLayout, getAdapter)
	{
	}

	protected override RecyclerViewScrollListener<ReorderableItemsView, IGroupableItemsViewSource> CreateScrollListener()
	{
		return new GestureScrollListener(ItemsView, ItemsViewAdapter);
	}

	public override bool DispatchTouchEvent(MotionEvent? e)
	{
		if (e is not null && isAtTop)
		{
			switch (e.Action)
			{
				case MotionEventActions.Up:
				case MotionEventActions.Cancel:
					if (isScrolling)
						PropagateEvent(e);
					isScrolling = false;
					break;

				case MotionEventActions.Down:
				case MotionEventActions.Move:
					if (e.HistorySize <= 0)
						goto END;

					isScrolling = true;

					var currentY = e.GetY();
					var previousY = e.GetHistoricalY(0);

					if (currentY - previousY > 0)
						PropagateEvent(e);
					break;
			}
		}
		END:
		return base.DispatchTouchEvent(e);


		void PropagateEvent(MotionEvent ev)
		{
			if (Parent is LayoutViewGroup viewGroup && viewGroup.CrossPlatformLayout is VisualElement visual)
				foreach (var b in visual.HandleGestureOnParents())
					b.HandleGestureFromChild(ev);
		}
	}

	class GestureScrollListener : RecyclerViewScrollListener<ReorderableItemsView, IGroupableItemsViewSource>
	{
		public GestureScrollListener(ReorderableItemsView itemsView, ItemsViewAdapter<ReorderableItemsView, IGroupableItemsViewSource> itemsViewAdapter) : base(itemsView, itemsViewAdapter)
		{
		}

		public override void OnScrolled(RecyclerView r, int dx, int dy)
		{
			base.OnScrolled(r, dx, dy);

			if (r is GestureRecyclerView recyclerView && recyclerView.GetLayoutManager() is LinearLayoutManager layoutManager)
			{
				var firstVisibleItemPosition = layoutManager.FindFirstVisibleItemPosition();
				recyclerView.isAtTop = firstVisibleItemPosition is 0;
			}
		}
	}
}


class GestureCollectionViewHandler : CollectionViewHandler
{
	protected override RecyclerView CreatePlatformView()
	{
		return new GestureRecyclerView(Context, GetItemsLayout, CreateAdapter);
	}
}
