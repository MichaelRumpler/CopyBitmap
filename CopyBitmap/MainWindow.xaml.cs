using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CopyBitmap
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		SKBitmap Bitmap;

		public MainWindow()
		{
			InitializeComponent();
		}

		#region Button Click events

		private void SkiaUp_Click(object sender, RoutedEventArgs e)
		{
			Copy.SkiaCopy(Bitmap, GetBottom(), GetTop());
			InvalidateSurface();
		}

		private void SkiaDown_Click(object sender, RoutedEventArgs e)
		{
			Copy.SkiaCopy(Bitmap, GetTop(), GetBottom());
			InvalidateSurface();
		}

		private void CopyManuallyUp_Click(object sender, RoutedEventArgs e)
		{
			Copy.ManualCopy(Bitmap, GetBottom(), GetTop());
			InvalidateSurface();
		}

		private void CopyManuallyDown_Click(object sender, RoutedEventArgs e)
		{
			Copy.ManualCopy(Bitmap, GetTop(), GetBottom());
			InvalidateSurface();
		}

		private void BufferUp_Click(object sender, RoutedEventArgs e)
		{
			Copy.BufferCopy(Bitmap, GetBottom(), GetTop());
			InvalidateSurface();
		}

		private void BufferDown_Click(object sender, RoutedEventArgs e)
		{
			Copy.BufferCopy(Bitmap, GetTop(), GetBottom());
			InvalidateSurface();
		}

		private void UnsafeUp_Click(object sender, RoutedEventArgs e)
		{
			Copy.UnsafeCopy(Bitmap, GetBottom(), GetTop());
			InvalidateSurface();
		}

		private void UnsafeDown_Click(object sender, RoutedEventArgs e)
		{
			Copy.UnsafeCopy(Bitmap, GetTop(), GetBottom());
			InvalidateSurface();
		}

		private void SafeUnsafeUp_Click(object sender, RoutedEventArgs e)
		{
			Copy.SafeUnsafeCopy(Bitmap, GetBottom(), GetTop());
			InvalidateSurface();
		}

		private void SafeUnsafeDown_Click(object sender, RoutedEventArgs e)
		{
			Copy.SafeUnsafeCopy(Bitmap, GetTop(), GetBottom());
			InvalidateSurface();
		}

		private void SpanUp_Click(object sender, RoutedEventArgs e)
		{
			Copy.SpanCopy(Bitmap, GetBottom(), GetTop());
			InvalidateSurface();
		}

		private void SpanDown_Click(object sender, RoutedEventArgs e)
		{
			Copy.SpanCopy(Bitmap, GetTop(), GetBottom());
			InvalidateSurface();
		}

		private void SpanUIntUp_Click(object sender, RoutedEventArgs e)
		{
			Copy.SpanUIntCopy(Bitmap, GetBottom(), GetTop());
			InvalidateSurface();
		}

		private void SpanUIntDown_Click(object sender, RoutedEventArgs e)
		{
			Copy.SpanUIntCopy(Bitmap, GetTop(), GetBottom());
			InvalidateSurface();
		}

		private void GenericSpanUp_Click(object sender, RoutedEventArgs e)
		{
			Copy.GenericSpanCopy(Bitmap, GetBottom(), GetTop());
			InvalidateSurface();
		}

		private void GenericSpanDown_Click(object sender, RoutedEventArgs e)
		{
			Copy.GenericSpanCopy(Bitmap, GetTop(), GetBottom());
			InvalidateSurface();
		}

		private void Reset_Click(object sender, RoutedEventArgs e)
		{
			var oldBitmap = Bitmap;
			Bitmap = InitBitmap();

			if (oldBitmap != null)
				oldBitmap.Dispose();

			InvalidateSurface();
		}

		#endregion Button Click events

		private void InvalidateSurface() => skElement.InvalidateVisual();

		private void SkElement_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
		{
			var bm = Bitmap;
			if (bm != null)
			{
				e.Surface.Canvas.DrawBitmap(bm, 0, 0);
			}
		}

		private void SkElement_SizeChanged(object sender, SizeChangedEventArgs e) => Reset_Click(sender, e);

		private SKBitmap InitBitmap()
		{
			var width = (float)(skElement.ActualWidth * GetDpiScaleX(skElement));
			var height = (float)(skElement.ActualHeight * GetDpiScaleY(skElement));

			var bitmap = new SKBitmap((int)width, (int)height);
			using (var canvas = new SKCanvas(bitmap))
			{
				using (var bg = new SKPaint())
				{
					for (int y = 0; y < height; y++)
					{
						bg.Color = new SKColor((byte)(255 * y / height), (byte)(255 - 255 * y / height), 0);
						canvas.DrawLine(0, y, width, y, bg);
					}
				}

				using (var fg = new SKPaint()
				{
					Typeface = SKTypeface.Default,
					TextSize = 14,
					IsAntialias = true,
					Color = SKColors.Blue,
					IsStroke = false,
				})
				{
					for (int y = 0, row = 0; y < height; y += (int)fg.FontSpacing, row++)
					{
						var str = string.Join(" ", Enumerable.Repeat(row.ToString(), 500));
						canvas.DrawText(str, 0, y - fg.FontMetrics.Ascent, fg);
					}
				}
				canvas.Flush();
			}
			return bitmap;
		}

		#region Helpers

		private SKRect GetTop()
		{
			return new SKRect(0, 0, Bitmap.Width, Bitmap.Height - Bitmap.Height / 10);
		}

		private SKRect GetBottom()
		{
			return new SKRect(0, Bitmap.Height / 10, Bitmap.Width, Bitmap.Height);
		}

		public static double GetDpiScaleX(Visual visual)
		{
			var dpi = VisualTreeHelper.GetDpi(visual);
			return dpi.DpiScaleX;
		}

		public static double GetDpiScaleY(Visual visual)
		{
			var dpi = VisualTreeHelper.GetDpi(visual);
			return dpi.DpiScaleY;
		}

		#endregion Helpers
	}
}
