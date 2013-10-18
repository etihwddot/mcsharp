using System.Collections;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace MCSharp.WorldBrowser.Views
{
	[ContentProperty("Content")]
	public sealed class PanZoomControl : FrameworkElement
	{
		public UIElement Content
		{
			get
			{
				return m_content;
			}
			set
			{
				if (m_content != null)
				{
					RemoveLogicalChild(m_content);
					RemoveVisualChild(m_content);
				}

				m_content = value;

				if (m_content != null)
				{
					AddLogicalChild(m_content);
					AddVisualChild(m_content);
				}
			}
		}

		protected override Visual GetVisualChild(int index)
		{
			return m_content;
		}

		protected override int VisualChildrenCount
		{
			get { return m_content != null ? 1 : 0; }
		}

		protected override IEnumerator LogicalChildren
		{
			get { return new[] { m_content }.GetEnumerator(); }
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			if (m_content == null)
				return new Size(0, 0);

			m_content.Measure(availableSize);
			return m_content.DesiredSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			if (m_content == null)
				return new Size(0, 0);

			m_content.Arrange(new Rect(finalSize));
			return m_content.RenderSize;
		}

		UIElement m_content;
	}
}
