using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using SkiaSharp;
using System;

namespace CopyBitmap.Benchmark
{
	public class CopyTests
	{
		public enum BitmapType
		{
			SmallAligned,
			SmallUnaligned,
			BigAligned,
			BigUnaligned,
		}

		private SKBitmap[] bitmaps;

		[GlobalSetup]
		public void Setup()
		{
			bitmaps = new SKBitmap[4];
			bitmaps[(int)BitmapType.SmallAligned] = new SKBitmap(128, 64);
			bitmaps[(int)BitmapType.SmallUnaligned] = new SKBitmap(127, 63);
			bitmaps[(int)BitmapType.BigAligned] = new SKBitmap(1920, 1080);
			bitmaps[(int)BitmapType.BigUnaligned] = new SKBitmap(1919, 1079);
		}

		[ParamsAllValues]
		public BitmapType Bitmap;

		[Benchmark]
		public void SkiaCopyDown()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.SkiaCopy(bitmap, GetTop(bitmap), GetBottom(bitmap));
		}

		[Benchmark]
		public void SkiaCopyUp()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.SkiaCopy(bitmap, GetBottom(bitmap), GetTop(bitmap));
		}

		[Benchmark]
		public void ManualCopyDown()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.ManualCopy(bitmap, GetTop(bitmap), GetBottom(bitmap));
		}

		[Benchmark]
		public void ManualCopyUp()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.ManualCopy(bitmap, GetBottom(bitmap), GetTop(bitmap));
		}

		[Benchmark]
		public void BufferCopyDown()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.BufferCopy(bitmap, GetTop(bitmap), GetBottom(bitmap));
		}

		[Benchmark]
		public void BufferCopyUp()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.BufferCopy(bitmap, GetBottom(bitmap), GetTop(bitmap));
		}

		[Benchmark]
		public void UnsafeCopyDown()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.UnsafeCopy(bitmap, GetTop(bitmap), GetBottom(bitmap));
		}

		[Benchmark]
		public void UnsafeCopyUp()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.UnsafeCopy(bitmap, GetBottom(bitmap), GetTop(bitmap));
		}

		[Benchmark]
		public void SpanCopyDown()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.SpanCopy(bitmap, GetTop(bitmap), GetBottom(bitmap));
		}

		[Benchmark]
		public void SpanCopyUp()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.SpanCopy(bitmap, GetBottom(bitmap), GetTop(bitmap));
		}

		[Benchmark]
		public void SpanUIntCopyDown()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.SpanUIntCopy(bitmap, GetTop(bitmap), GetBottom(bitmap));
		}

		[Benchmark]
		public void SpanUIntCopyUp()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.SpanUIntCopy(bitmap, GetBottom(bitmap), GetTop(bitmap));
		}

		[Benchmark]
		public void GenericSpanCopyDown()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.GenericSpanCopy(bitmap, GetTop(bitmap), GetBottom(bitmap));
		}

		[Benchmark]
		public void GenericSpanCopyUp()
		{
			var bitmap = bitmaps[(int)Bitmap];
			Copy.GenericSpanCopy(bitmap, GetBottom(bitmap), GetTop(bitmap));
		}

		#region Helpers

		private SKRect GetTop(SKBitmap Bitmap)
		{
			return new SKRect(0, 0, Bitmap.Width, Bitmap.Height - Bitmap.Height / 16);
		}

		private SKRect GetBottom(SKBitmap Bitmap)
		{
			return new SKRect(0, Bitmap.Height / 16, Bitmap.Width, Bitmap.Height);
		}

		#endregion Helpers
	}

	class Program
	{
		static void Main(string[] args)
		{
			
			var summary = BenchmarkRunner.Run<CopyTests>(new BenchmarkDotNet.Configs.DebugBuildConfig());
		}
	}
}
