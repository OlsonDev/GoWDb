using System;
using System.IO;
using System.Text;
using ImageProcessorCore;
using Spriter.Extensions;

namespace Spriter.Models {
	public class AtfRawCompressedAlpha {
		public uint Dxt5ImageDataLength { get; private set; }
		public byte[] Dxt5ImageData { get; private set; }
		public uint PvrtcImageDataLength { get; private set; }
		public byte[] PvrtcImageData { get; private set; }
		public uint Etc1ImageDataLength { get; private set; }
		public uint Etc1ImageDataLengthHalf => Etc1ImageDataLength / 2;
		public byte[] Etc1ImageData { get; private set; }
		public byte[] Etc1AlphaImageData { get; private set; }
		public uint Etc2RgbaImageDataLength { get; private set; }
		public byte[] Etc2RgbaImageData { get; private set; }

		public void SaveDxt5ImageDataAsPng(int width, int height, string path) {
			var image = new Image(width, height);
			var pixels = image.Pixels;
			var reader = new BinaryReader(new MemoryStream(Dxt5ImageData));
			var numBlocksHigh = height / 4;
			var numBlocksWide = width / 4;

			for (var by = 0; by < numBlocksHigh; by++) {
				for (var bx = 0; bx < numBlocksWide; bx++) {
					var alphas = BuildAlphas(reader.ReadByte(), reader.ReadByte());
					var a0 = reader.ReadUInt16();
					var a1 = reader.ReadUInt16();
					var a2 = reader.ReadUInt16();
					var alphaIndices = new [] { a2, a1, a0 };
					var colors = AtfFile.BuildColors(reader.ReadUInt16(), reader.ReadUInt16());
					var colorIndices = reader.ReadUInt32();
					for (var y = 0; y < 4; y++) {
						for (var x = 0; x < 4; x++) {
							var blockIndex = y * 4 + 3 - x;
							var alphaIndex = GetAlphaIndex(alphaIndices, blockIndex);
							var alpha = alphas[alphaIndex];
							var color = colors[(colorIndices >> (2 * (15 - blockIndex))) & 0x03];
							var pixelIndex = (by * 4 + 3 - y) * width + bx * 4 + x;
							pixels[pixelIndex] = new Color(color.R, color.G, color.B, alpha);
						}
					}
				}
			}

			image.Save(File.OpenWrite(path));
			Console.WriteLine($"Saved file {path}");
		}

		public int GetAlphaIndex(UInt16[] alphaIndices, int blockIndex) {
			return ExtractBitsFromUInt16Array(alphaIndices, 3 * (15 - blockIndex), 3);
		}

		public int ExtractBitsFromUInt16Array(UInt16[] array, int shift, int length) {
			var lastIndex = array.Length - 1;
			var width = 16;
			var rowS = shift / width;
			var rowE = (shift + length - 1) / width;
			var mask = (UInt16)(Math.Pow(2, length) - 1);
			var shiftS = shift % width;
			var bits = (array[lastIndex - rowS] >> shiftS) & mask;
			if (rowS == rowE) return bits; // Single UInt16
			// Two contiguous UInt16s
			var shiftE = width - shiftS;
			var mask2 = (int)Math.Pow(2, length - shiftE) - 1;
			return bits + ((array[lastIndex - rowE] & mask2) << shiftE);
		}

		public byte[] BuildAlphas(byte a0, byte a1) {
			var alphas = new byte[8];
			alphas[0] = a0;
			alphas[1] = a1;

			var numAlphasToCompute = 6;
			if (a0 <= a1) {
				numAlphasToCompute = 4;
				alphas[6] = 0;
				alphas[7] = 255;
			}
			int denominator = numAlphasToCompute + 1;
			var n = 2 + numAlphasToCompute;
			var a0f = numAlphasToCompute;
			var a1f = 1;
			for (var i = 2; i < n; i++) {
				alphas[i] = (byte)((a0f * a0 + a1f * a1) / denominator);
				a0f--;
				a1f++;
			}
			return alphas;
		}

		public override string ToString() {
			var builder = new StringBuilder();
			builder.AppendLine($"dxt5  data length : {Dxt5ImageDataLength}");
			builder.AppendLine($"pvrtc data length : {PvrtcImageDataLength}");
			builder.AppendLine($"etc1  data length : {Etc1ImageDataLengthHalf}");
			builder.AppendLine($"etc2  rgba length : {Etc2RgbaImageDataLength}");
			return builder.ToString();
		}

		public static AtfRawCompressedAlpha[] FromBytes(BinaryReader reader, int count) {
			var array = new AtfRawCompressedAlpha[count];
			for (var i = 0; i < count; i++) {
				var data = new AtfRawCompressedAlpha();

				data.Dxt5ImageDataLength = reader.ReadUInt32BE();
				data.Dxt5ImageData = reader.ReadBytes(checked((int)data.Dxt5ImageDataLength));

				data.PvrtcImageDataLength = reader.ReadUInt32BE();
				data.PvrtcImageData = reader.ReadBytes(checked((int)data.PvrtcImageDataLength));

				data.Etc1ImageDataLength = reader.ReadUInt32BE();
				data.Etc1ImageData = reader.ReadBytes(checked((int)data.Etc1ImageDataLengthHalf));
				data.Etc1AlphaImageData = reader.ReadBytes(checked((int)data.Etc1ImageDataLengthHalf));

				data.Etc2RgbaImageDataLength = reader.ReadUInt32BE();
				data.Etc2RgbaImageData = reader.ReadBytes(checked((int)data.Etc2RgbaImageDataLength));

				array[i] = data;
			}
			return array;
		}
	}
}