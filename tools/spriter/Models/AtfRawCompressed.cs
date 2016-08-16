using System;
using System.IO;
using System.Text;
using ImageProcessorCore;
using Spriter.Extensions;

namespace Spriter.Models {
	public class AtfRawCompressed {
		public uint Dxt1ImageDataLength { get; private set; }
		public byte[] Dxt1ImageData { get; private set; }
		public uint PvrtcImageDataLength { get; private set; }
		public byte[] PvrtcImageData { get; private set; }
		public uint Etc1ImageDataLength { get; private set; }
		public uint Etc1ImageDataLengthHalf => Etc1ImageDataLength / 2;
		public byte[] Etc1ImageData { get; private set; }
		public byte[] Etc1AlphaImageData { get; private set; }
		public uint Etc2RgbaImageDataLength { get; private set; }
		public byte[] Etc2RgbaImageData { get; private set; }

		public void SaveDxt1ImageDataAsPng(int width, int height, string path) {
			var image = new Image(width, height);
			var pixels = image.Pixels;
			var reader = new BinaryReader(new MemoryStream(Dxt1ImageData));
			var numBlocksHigh = height / 4;
			var numBlocksWide = width / 4;

			for (var by = 0; by < numBlocksHigh; by++) {
				for (var bx = 0; bx < numBlocksWide; bx++) {
					var colors = AtfFile.BuildColors(reader.ReadUInt16(), reader.ReadUInt16());
					var colorIndices = reader.ReadUInt32();
					for (var y = 0; y < 4; y++) {
						for (var x = 0; x < 4; x++) {
							var blockIndex = y * 4 + 3 - x;
							var color = colors[(colorIndices >> (2 * (15 - blockIndex))) & 0x03];
							var pixelIndex = (by * 4 + 3 - y) * width + bx * 4 + x;
							pixels[pixelIndex] = color;
						}
					}
				}
			}

			image.Save(File.OpenWrite(path));
			Console.WriteLine($"Saved file {path}");
		}

		public override string ToString() {
			var builder = new StringBuilder();
			builder.AppendLine($"dxt1  data length : {Dxt1ImageDataLength}");
			builder.AppendLine($"pvrtc data length : {PvrtcImageDataLength}");
			builder.AppendLine($"etc1  data length : {Etc1ImageDataLengthHalf}");
			builder.AppendLine($"etc2  rgba length : {Etc2RgbaImageDataLength}");
			return builder.ToString();
		}

		public static AtfRawCompressed[] FromBytes(BinaryReader reader, int count) {
			var array = new AtfRawCompressed[count];
			for (var i = 0; i < count; i++) {
				var data = new AtfRawCompressed();

				data.Dxt1ImageDataLength = reader.ReadUInt32BE();
				data.Dxt1ImageData = reader.ReadBytes(checked((int)data.Dxt1ImageDataLength));

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