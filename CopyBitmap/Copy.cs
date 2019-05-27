using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace CopyBitmap
{
	public static class Copy
	{
		public static void SkiaCopy(SKBitmap Bitmap, SKRect source, SKRect target)
		{
			using (var canvas = new SKCanvas(Bitmap))
			{
				canvas.DrawBitmap(Bitmap, source, target);
				canvas.Flush();
			}
		}

		public static void ManualCopy(SKBitmap Bitmap, SKRect source, SKRect target)
		{
			if (Bitmap.BytesPerPixel != 4)      // just to be sure, may have to work for other sizes too
				throw new NotImplementedException();

			var width = (int)source.Width;
			var height = (int)source.Height;

			using (var canvas = new SKCanvas(Bitmap))
			{
				if (width == Bitmap.Width)
				{
					width = width * height;
					height = 1;
				}

				var sourceIndex = (int)(source.Top * Bitmap.Width + source.Left);                // start of first line
				var targetIndex = (int)(target.Top * Bitmap.Width + target.Left);

				int lineDelta = Bitmap.Width - width;
				int proceed = +1;

				if (source.IntersectsWith(target)           // it they intersect
					&& sourceIndex < targetIndex)           // and the source should be copied down
				{
					sourceIndex += (height - 1) * Bitmap.Width + width - 1;						 // last pixel of the last line
					targetIndex += (height - 1) * Bitmap.Width + width - 1;

					lineDelta = -lineDelta;
					proceed = -1;
				}

				IntPtr pixelsAddr = Bitmap.GetPixels();

				unsafe
				{
					uint* ptr = (uint*)pixelsAddr.ToPointer();
					uint* sourcePtr = ptr + sourceIndex;
					uint* targetPtr = ptr + targetIndex;

					for (int y = height - 1; y >= 0; y--)
					{
						for (int x = width - 1; x >= 0; x--)
						{
							*targetPtr = *sourcePtr;
							sourcePtr += proceed;
							targetPtr += proceed;
						}
						sourcePtr += lineDelta;
						targetPtr += lineDelta;
					}
				}


				canvas.Flush();
			}
		}

		public static void BufferCopy(SKBitmap Bitmap, SKRect source, SKRect target)
		{
			var width = (int)source.Width;
			var height = (int)source.Height;

			using (var canvas = new SKCanvas(Bitmap))
			{
				if (width == Bitmap.Width)
				{
					width = width * height;
					height = 1;
				}

				var sourceIndex = (int)(source.Top * Bitmap.Width + source.Left) * Bitmap.BytesPerPixel;                // start of first line
				var targetIndex = (int)(target.Top * Bitmap.Width + target.Left) * Bitmap.BytesPerPixel;

				var bytesToCopy = (uint)(width * Bitmap.BytesPerPixel);
				var bytesPerRow = Bitmap.RowBytes;

				if (source.IntersectsWith(target)           // it they intersect
					&& sourceIndex < targetIndex)           // and the source should be copied down
				{
					sourceIndex += (height - 1) * Bitmap.Width * Bitmap.BytesPerPixel;                                  // start of the last line
					targetIndex += (height - 1) * Bitmap.Width * Bitmap.BytesPerPixel;

					bytesPerRow = -Bitmap.RowBytes;     // go back after each line
				}

				IntPtr pixelsAddr = Bitmap.GetPixels();

				unsafe
				{
					for (int y = 0; y < height; y++)
					{
						Buffer.MemoryCopy((void*)(pixelsAddr + sourceIndex), (void*)(pixelsAddr + targetIndex), bytesToCopy, bytesToCopy);

						sourceIndex += bytesPerRow;
						targetIndex += bytesPerRow;
					}
				}


				canvas.Flush();
			}
		}

		public static void UnsafeCopy(SKBitmap Bitmap, SKRect source, SKRect target)
		{
			var width = (int)source.Width;
			var height = (int)source.Height;

			using (var canvas = new SKCanvas(Bitmap))
			{
				IntPtr pixelsAddr = Bitmap.GetPixels();
				//ref byte pixelsPtr = Unsafe.AsRef<byte>((void*)pixelsAddr);
				if (width == Bitmap.Width)
				{
					width = width * height;
					height = 1;
				}

				var sourceEnd = (int)((source.Top + height - 1) * Bitmap.Width + source.Left) * Bitmap.BytesPerPixel;   // start of the last line
				var targetEnd = (int)((target.Top + height - 1) * Bitmap.Width + target.Left) * Bitmap.BytesPerPixel;

				// MemoryCopy checks for overlapping
				var bytesPerLine = (uint)(width * Bitmap.BytesPerPixel);

				if (source.IntersectsWith(target)           // it they intersect
					&& sourceEnd < targetEnd)                       // and the source should be copied down
				{
					unsafe
					{
						for (int y = height - 1; y >= 0; y--)
						{
							Unsafe.CopyBlockUnaligned((void*)(pixelsAddr + targetEnd), (void*)(pixelsAddr + sourceEnd), bytesPerLine);
							//Unsafe.CopyBlockUnaligned((void*)(pixelsAddr + targetEnd), (void*)(pixelsAddr + sourceEnd), bytesPerLine);
							sourceEnd -= Bitmap.RowBytes;
							targetEnd -= Bitmap.RowBytes;
						}
					}
				}
				else
				{
					var sourceStart = (int)(source.Top * Bitmap.Width + source.Left) * Bitmap.BytesPerPixel;
					var targetStart = (int)(target.Top * Bitmap.Width + target.Left) * Bitmap.BytesPerPixel;

					unsafe
					{
						for (int y = 0; y < height; y++)
						{
							Unsafe.CopyBlockUnaligned((void*)(pixelsAddr + targetEnd), (void*)(pixelsAddr + sourceEnd), bytesPerLine);
							sourceStart += Bitmap.RowBytes;
							targetStart += Bitmap.RowBytes;
						}
					}
				}


				canvas.Flush();
			}
		}

		public static void SafeUnsafeCopy(SKBitmap Bitmap, SKRect source, SKRect target)
		{
			var width = (int)source.Width;
			var height = (int)source.Height;

			using (var canvas = new SKCanvas(Bitmap))
			{
				if (width == Bitmap.Width)
				{
					width = width * height;
					height = 1;
				}

				var sourceIndex = (int)(source.Top * Bitmap.Width + source.Left) * Bitmap.BytesPerPixel;                // start of first line
				var targetIndex = (int)(target.Top * Bitmap.Width + target.Left) * Bitmap.BytesPerPixel;

				var bytesToCopy = (uint)(width * Bitmap.BytesPerPixel);
				var bytesPerRow = Bitmap.RowBytes;

				if (source.IntersectsWith(target)           // it they intersect
					&& sourceIndex < targetIndex)           // and the source should be copied down
				{
					sourceIndex += (height - 1) * Bitmap.Width * Bitmap.BytesPerPixel;                                  // start of the last line
					targetIndex += (height - 1) * Bitmap.Width * Bitmap.BytesPerPixel;

					bytesPerRow = -Bitmap.RowBytes;     // go back after each line
				}

				// throws
				ref byte pixelsRef = ref Bitmap.GetPixelRef(out var _);

				for (int y = 0; y < height; y++)
				{
					Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref pixelsRef, targetIndex), ref Unsafe.Add(ref pixelsRef, sourceIndex), bytesToCopy);

					sourceIndex += bytesPerRow;
					targetIndex += bytesPerRow;
				}
				

				canvas.Flush();
			}
		}

		public static void SpanCopy(SKBitmap Bitmap, SKRect source, SKRect target)
		{
			var width = (int)source.Width;
			var height = (int)source.Height;

			using (var canvas = new SKCanvas(Bitmap))
			{
				if (width == Bitmap.Width)
				{
					width = width * height;
					height = 1;
				}

				var sourceIndex = (int)(source.Top * Bitmap.Width + source.Left) * Bitmap.BytesPerPixel;				// start of first line
				var targetIndex = (int)(target.Top * Bitmap.Width + target.Left) * Bitmap.BytesPerPixel;

				var bytesToCopy = width * Bitmap.BytesPerPixel;
				var bytesPerRow = Bitmap.RowBytes;

				if (source.IntersectsWith(target)           // it they intersect
					&& sourceIndex < targetIndex)           // and the source should be copied down
				{
					sourceIndex += (height - 1) * Bitmap.Width * Bitmap.BytesPerPixel;									// start of the last line
					targetIndex += (height - 1) * Bitmap.Width * Bitmap.BytesPerPixel;

					bytesPerRow = -Bitmap.RowBytes;		// go back after each line
				}

				var pixelsSpan = Bitmap.GetPixelSpan();

				for (int y = 0; y < height; y++)
				{
					pixelsSpan.Slice(sourceIndex, bytesToCopy)
						.CopyTo(pixelsSpan.Slice(targetIndex, bytesToCopy));

					sourceIndex += bytesPerRow;
					targetIndex += bytesPerRow;
				}

				canvas.Flush();
			}
		}

		public static void SpanUIntCopy(SKBitmap Bitmap, SKRect source, SKRect target)
		{
			if (Bitmap.BytesPerPixel != 4)      // just to be sure, may have to work for other sizes too
				throw new NotImplementedException();

			var width = (int)source.Width;
			var height = (int)source.Height;

			using (var canvas = new SKCanvas(Bitmap))
			{
				if (width == Bitmap.Width)
				{
					width = width * height;
					height = 1;
				}

				var sourceIndex = (int)(source.Top * Bitmap.Width + source.Left);                // start of first line
				var targetIndex = (int)(target.Top * Bitmap.Width + target.Left);

				var pixelsToCopy = width;
				var pixelsPerRow = Bitmap.Width;

				if (source.IntersectsWith(target)           // it they intersect
					&& sourceIndex < targetIndex)           // and the source should be copied down
				{
					sourceIndex += (height - 1) * Bitmap.Width;                                  // start of the last line
					targetIndex += (height - 1) * Bitmap.Width;

					pixelsPerRow = -Bitmap.Width;     // go back after each line
				}

				var pixelsSpan = Bitmap.GetPixelSpanUInt();

				for (int y = 0; y < height; y++)
				{
					pixelsSpan.Slice(sourceIndex, pixelsToCopy)
						.CopyTo(pixelsSpan.Slice(targetIndex, pixelsToCopy));

					sourceIndex += pixelsPerRow;
					targetIndex += pixelsPerRow;
				}

				canvas.Flush();
			}
		}

		public static void GenericSpanCopy(SKBitmap Bitmap, SKRect source, SKRect target)
		{
			if (Bitmap.BytesPerPixel != 4)      // just to be sure, may have to work for other sizes too
				throw new NotImplementedException();

			var width = (int)source.Width;
			var height = (int)source.Height;

			using (var canvas = new SKCanvas(Bitmap))
			{
				if (width == Bitmap.Width)
				{
					width = width * height;
					height = 1;
				}

				var sourceIndex = (int)(source.Top * Bitmap.Width + source.Left);                // start of first line
				var targetIndex = (int)(target.Top * Bitmap.Width + target.Left);

				var pixelsToCopy = width;
				var pixelsPerRow = Bitmap.Width;

				if (source.IntersectsWith(target)           // it they intersect
					&& sourceIndex < targetIndex)           // and the source should be copied down
				{
					sourceIndex += (height - 1) * Bitmap.Width;                                  // start of the last line
					targetIndex += (height - 1) * Bitmap.Width;

					pixelsPerRow = -Bitmap.Width;     // go back after each line
				}

				var pixelsSpan = Bitmap.GetPixelSpan<uint>();

				for (int y = 0; y < height; y++)
				{
					pixelsSpan.Slice(sourceIndex, pixelsToCopy)
						.CopyTo(pixelsSpan.Slice(targetIndex, pixelsToCopy));

					sourceIndex += pixelsPerRow;
					targetIndex += pixelsPerRow;
				}

				canvas.Flush();
			}
		}


		public static ref byte GetPixelRef(this SKBitmap bitmap, out IntPtr length)
		{
			// doesn't work :-(
			// how can I cast a IntPtr to a ref byte?
			return ref SkiaApi.sk_bitmap_get_pixels_ref(bitmap.Handle, out length);
		}

		public static unsafe Span<byte> GetPixelSpan(this SKBitmap bitmap)
		{
			var ptr = SkiaApi.sk_bitmap_get_pixels(bitmap.Handle, out var length);
			return new Span<byte>(ptr.ToPointer(), length.ToInt32());
		}

		public static unsafe Span<uint> GetPixelSpanUInt(this SKBitmap bitmap)
		{
			var ptr = SkiaApi.sk_bitmap_get_pixels(bitmap.Handle, out var length);
			return new Span<uint>(ptr.ToPointer(), length.ToInt32());
		}

		// this allows something like GetPixelSpan<Foobar>()
		public static unsafe Span<T> GetPixelSpan<T>(this SKBitmap bitmap)
		{
			var ptr = SkiaApi.sk_bitmap_get_pixels(bitmap.Handle, out var length);
			return new Span<T>(ptr.ToPointer(), length.ToInt32());
		}
	}
}