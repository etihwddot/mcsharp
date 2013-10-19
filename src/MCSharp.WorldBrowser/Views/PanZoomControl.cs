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
			m_translateTransform = new TranslateTransform();
			ClipToBounds = true;
		}

		public FrameworkElement Content
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
					TransformGroup transformGroup = new TransformGroup();
					transformGroup.Children.Add(m_scaleTransform);
					transformGroup.Children.Add(m_translateTransform);
					m_content.RenderTransform = transformGroup;
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
			return availableSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			if (m_content == null)
				return new Size(0, 0);

			m_content.Arrange(new Rect(finalSize));
			return finalSize;
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			// calculate initial bounds of content
			// calculate the initial position of the mouse
			Point mousePoint = e.GetPosition(m_content);
			Point initialPoint = m_scaleTransform.Transform(mousePoint);
			
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

			// calculate the new location of the mouse
			Point finalMousePoint = m_scaleTransform.Transform(mousePoint);

			double widthChange = initialPoint.X - finalMousePoint.X;
			double offsetWidthChange = widthChange;
			CoerceTranslateX(m_translateTransform.X + offsetWidthChange);

			double heightChange = initialPoint.Y - finalMousePoint.Y;
			double offsetHeightChange = heightChange;
			CoerceTranslateY(m_translateTransform.Y + offsetHeightChange);
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.SizeAll;
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			Mouse.OverrideCursor = null;
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			m_captureStart = e.GetPosition(this);
			m_transformXStart = m_translateTransform.X;
			m_transformYStart = m_translateTransform.Y;
			Mouse.Capture(this);
			e.Handled = true;
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			Mouse.Capture(null);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (Mouse.Captured == this && m_content != null)
			{
				Vector vector = e.GetPosition(this) - m_captureStart;
								
				CoerceTranslateX(m_transformXStart + vector.X);
				CoerceTranslateY(m_transformYStart + vector.Y);
			}
		}

		private void CoerceTranslateX(double newX)
		{
			Rect contentBounds = m_scaleTransform.TransformBounds(VisualTreeHelper.GetContentBounds(m_content));

			double widthDifference = Math.Min(0, m_content.ActualWidth - contentBounds.Width);
			m_translateTransform.X = LimitValue(newX, widthDifference, 0);
		}

		private void CoerceTranslateY(double newY)
		{
			Rect contentBounds = m_scaleTransform.TransformBounds(VisualTreeHelper.GetContentBounds(m_content));

			double heightDifference = Math.Min(0, m_content.ActualHeight - contentBounds.Height);
			m_translateTransform.Y = LimitValue(newY, heightDifference, 0);
		}

		private double LimitValue(double value, double minValue, double maxValue)
		{
			if (value > maxValue)
				value = maxValue;
			else if (value < minValue)
				value = minValue;

			return value;
		}

		const double c_scaleIncrement = 0.5;

		FrameworkElement m_content;
		ScaleTransform m_scaleTransform;
		TranslateTransform m_translateTransform;
		
		Point m_captureStart;
		double m_transformXStart;
		double m_transformYStart;
	}
}
