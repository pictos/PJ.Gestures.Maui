using Microsoft.Maui.Controls.Handlers.Items;
using UIKit;

namespace PJ.Gestures.Maui.Samples.Controls;

class GestureReorderableItemsViewController : ReorderableItemsViewController<ReorderableItemsView>
{
	public GestureReorderableItemsViewController(ReorderableItemsView reorderableItemsView, ItemsViewLayout layout) : base(reorderableItemsView, layout)
	{
	}

	protected override UICollectionViewDelegateFlowLayout CreateDelegator()
	{
		return new GestureReorderableItemsViewDelegator(ItemsViewLayout, this);
	}

	class GestureReorderableItemsViewDelegator : ReorderableItemsViewDelegator<ReorderableItemsView,
		ReorderableItemsViewController<ReorderableItemsView>>
	{
		public GestureReorderableItemsViewDelegator(ItemsViewLayout itemsViewLayout, ReorderableItemsViewController<ReorderableItemsView> itemsViewController) : base(itemsViewLayout, itemsViewController)
		{
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			var visualElement = ViewController.ItemsView;

			if (ViewController.View?.Subviews[0] is not UICollectionView uiCollectionView)
			{
				goto END;
			}

			var behaviors = visualElement.HandleGestureOnParents().ToArray();

			if (behaviors.Length is 0)
			{
				goto END;
			}

			var firstVisibleItem = uiCollectionView.IndexPathsForVisibleItems.Contains(Foundation.NSIndexPath.FromRowSection(0, 0));

			if (!firstVisibleItem)
			{
				goto END;
			}

			var panGesture = uiCollectionView.PanGestureRecognizer;

			foreach (var b in behaviors)
				b.HandleGestureFromChild(panGesture);

			END:
			base.Scrolled(scrollView);
		}
	}
}


class GestureCollectionViewHandler : CollectionViewHandler
{
	protected override ItemsViewController<ReorderableItemsView> CreateController(ReorderableItemsView itemsView, ItemsViewLayout layout)
	{
		return new GestureReorderableItemsViewController(itemsView, layout);
	}
}
