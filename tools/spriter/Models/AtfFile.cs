using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ImageProcessorCore;
using Spriter.Extensions;

namespace Spriter.Models {
	public class AtfFile {
		private byte[] signature;
		private byte[] reserved;
		private byte version;
		private uint length;
		private byte cubemapAndFormat;
		private byte log2Width;
		private byte log2Height;
		private byte count;
		private object[] textureData;
		private string filename;

		public bool IsCubemap => Convert.ToBoolean(cubemapAndFormat & 0x80);
		public AtfFormat Format => (AtfFormat)(cubemapAndFormat & 0x7F);

		public int Width => (int)Math.Pow(2, log2Width);
		public int Height => (int)Math.Pow(2, log2Height);

		public override string ToString() {
			// Example output from `atfinfo -i BannersSmall.atf`
			// File Name          : BannersSmall.atf
			// ATF Version        : 3
			// ATF File Type      : RAW Compressed With Alpha (DXT5)
			// Size               : 512x1024
			// Cube Map           : no
			// Empty Mipmaps      : no
			// Actual Mipmaps     : 1
			// Embedded Levels    : X.......... (512x1024)
			// AS3 Texture Class  : Texture (flash.display3D.Texture)
			// AS3 Texture Format : Context3DTextureFormat.COMPRESSED (flash.display3D.Context3DTextureFormat)

			// Example output from docs:
			// C:\> atfinfo -i test.atf
			// File Name          : test.atf
			// ATF File Type      : Compressed (DXT1+ETC1+PVRTC4bpp)
			// ATF Version        : 2
			// Size               : 1024x1024
			// Cube Map           : no
			// Empty Mipmaps      : no
			// Actual Mipmaps     : 10
			// Embedded Levels    : .XXXXXXXXXX (512x512,256x256,128x128,64x64,32x32,16x16,8x8,4x4,2x2,1x1)
			// AS3 Texture Class  : Texture (flash.display3D.Texture)
			// AS3 Texture Format : Context3DTextureFormat.COMPRESSED (flash.display3D.Context3DTextureFormat)

			var builder = new StringBuilder();
			builder.AppendLine($"File name         : {Path.GetFileName(filename)}");
			builder.AppendLine($"ATF version       : {version}");
			builder.AppendLine($"ATF file type     : {Format.GetDisplayDescription()}");
			builder.AppendLine($"Size              : {Width}x{Height}");
			builder.AppendLine($"Length            : {length}");
			builder.AppendLine($"Count             : {count}");
			builder.AppendLine($"Cube map          : {(IsCubemap ? "yes" : "no")}");
			builder.AppendLine($"Empty mipmaps     : <not implemented>");
			builder.AppendLine($"Actual mipmaps    : <not implemented>");
			builder.AppendLine($"Embedded levels   : <not implemented>");
			builder.AppendLine($"AS3 texture class : <not implemented>");
			builder.AppendLine($"AS3 texture format: <not implemented>");

			builder.AppendLine($"=========================================");
			foreach (var data in textureData.Cast<AtfRawCompressedAlpha>()) {
				if (data.Dxt5ImageDataLength > 0) {
					File.WriteAllBytes(@".\output.dxt5.hex", data.Dxt5ImageData);
					data.ProcessDxt5ImageData(Width, Height);
				}
				builder.AppendLine($"dxt5  data length : {data.Dxt5ImageDataLength}");
				builder.AppendLine($"pvrtc data length : {data.PvrtcImageDataLength}");
				builder.AppendLine($"etc1  data length : {data.Etc1ImageDataLengthHalf}");
				builder.AppendLine($"etc2  rgba length : {data.Etc2RgbaImageDataLength}");
				builder.AppendLine($"=========================================");
			}



			return builder.ToString();
		}

		public static AtfFile FromFile(string path) {
			using (var stream = File.OpenRead(path)) {
				var file = FromStream(stream);
				file.filename = path;
				return file;
			}
		}

		public static AtfFile FromStream(Stream stream) {
			var reader = new BinaryReader(stream);
			var file = new AtfFile();
			// See https://www.adobe.com/devnet/archive/flashruntimes/articles/atf-file-format.html
			// Example from BannersSmall.atf:
			// 4154 4600 0002 ff03 0008 00b4 0509 0a0b
			// 0008 0000 f900 4992 24c9 9000 201b 0000
			file.signature = reader.ReadBytes(3); // 0x415446; ASCII 'ATF' chars
			file.reserved = reader.ReadBytes(4); // 0x000002ff
			file.version = reader.ReadByte(); // 0x03
			file.length = reader.ReadUInt32BE(); // 0x000800b4; 524,480 byte file
			file.cubemapAndFormat = reader.ReadByte();
			file.log2Width = reader.ReadByte();
			file.log2Height = reader.ReadByte();
			file.count = reader.ReadByte();
			file.textureData = BuildTextureData(file, reader, file.count);
			return file;
		}

		public static object[] BuildTextureData(AtfFile file, BinaryReader reader, int count) {
			switch (file.Format) {
				case AtfFormat.ATFRAWCOMPRESSEDALPHA: return AtfRawCompressedAlpha.FromBytes(reader, count * (file.IsCubemap ? 6 : 1));
			}
			throw new NotSupportedException($"Cannot build texture data for format {file.Format.GetDisplayDescription()}");
		}
	}

	public enum AtfFormat {
		[Display(Description="RGB888")]
		ATFRGB888 = 0x00,
		[Display(Description="RGBA88888")]
		ATFRGBA8888 = 0x01,
		[Display(Description="Compressed")]
		ATFCOMPRESSED = 0x02,
		[Display(Description="RAW Compressed")]
		ATFRAWCOMPRESSED = 0x03,
		[Display(Description="Compressed with Alpha")]
		ATFCOMPRESSEDALPHA = 0x04,
		[Display(Description="RAW Compressed with Alpha")]
		ATFRAWCOMPRESSEDALPHA = 0x05,
		[Display(Description="Compressed Lossy")]
		ATFCOMPRESSEDLOSSY = 0x0c,
		[Display(Description="Compressed Lossy with Alpha")]
		ATFALPHACOMPRESSEDLOSSY = 0x0d
	}

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

		public void ProcessDxt5ImageData(int width, int height) {
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
					var colors = BuildColors(reader.ReadUInt16(), reader.ReadUInt16());
					var colorIndices = reader.ReadUInt32BE();
					for (var y = 0; y < 4; y++) {
						for (var x = 0; x < 4; x++) {
							var blockIndex = y * 4 + 3 - x;
							var alphaIndex = GetAlphaIndex(alphaIndices, blockIndex);
							var alpha1 = alphas[alphaIndex];
							var color = colors[(colorIndices >> (2 * (15 - blockIndex))) & 0x03];
							var pixelIndex = (by * 4 + y) * width + bx * 4 + x;
							pixels[pixelIndex] = new Color(color.R, color.G, color.B, alpha1);
						}
					}
				}
			}

			image.Save(File.OpenWrite(@".\output.png"));
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
			var a1f = numAlphasToCompute;
			var a0f = 1;
			for (var i = 2; i < n; i++) {
				alphas[i] = (byte)((a0f * a0 + a1f * a1) / denominator);
				a0f++;
				a1f--;
			}
			return alphas;
		}

		public Color[] BuildColors(UInt16 color0raw, UInt16 color1raw) {
			var colors = new Color[4];
			var c0 = colors[0] = BuildColorFrom565Short(color0raw);
			var c1 = colors[1] = BuildColorFrom565Short(color1raw);
			if (color0raw > color1raw) {
				colors[2] = Color.Lerp(c0, c1, 1.0f / 3.0f);
				colors[3] = Color.Lerp(c0, c1, 2.0f / 3.0f);
			} else {
				colors[2] = Color.Lerp(c0, c1, 0.5f);
				colors[3] = new Color(0, 0, 0, 0);
			}
			return colors;
		}

		public Color BuildColorFrom565Short(UInt16 color) {
			var r = (color & 0xF800) >> 11;
			var g = (color & 0x07E0) >> 5;
			var b = (color & 0x001F) >> 0;
			return new Color(r / 31.0f, g / 63.0f, b / 31.0f);
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