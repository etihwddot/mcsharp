using System;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace MCSharp.WorldBrowser.Views
{
	[ContentProperty("Content")]
	public sealed class PanZoomControl : FrameworkElement
	{
		public PanZoomControl()
		{
			m_scaleTransform = new ScaleTransform();
		}

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
					m_scaleTransform.ScaleX = 1;
					m_scaleTransform.ScaleY = 1;

					AddLogicalChild(m_content);
					AddVisualChild(m_content);
					m_content.RenderTransform = m_scaleTransform;
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

		protected override void OnRender(DrawingContext drawingContext)
		{
			drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			Point mousePoint = e.GetPosition(this);

			if (e.Delta > 0)
			{
				m_scaleTransform.ScaleX += c_scaleIncrement;
				m_scaleTransform.ScaleY += c_scaleIncrement;
			}
			else
			{
				m_scaleTransform.ScaleX = Math.Max(1, m_scaleTransform.ScaleX - c_scaleIncrement);
				m_scaleTransform.ScaleY = Math.Max(1, m_scaleTransform.ScaleY - c_scaleIncrement);
			}
		}

		const double c_scaleIncrement = 0.5;

		UIElement m_content;
		ScaleTransform m_scaleTransform;
	}
}
