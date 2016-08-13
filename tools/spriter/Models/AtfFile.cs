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
					File.WriteAllBytes(@"C:\output.dxt5.hex", data.Dxt5ImageData);
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

		// public void ProcessDxt5ImageData() {
		// 	var w = 512;
		// 	var h = 1024;
		// 	var image = new Image(w, h);
		// 	var pixels = image.Pixels;

		// 	var reader = new BinaryReader(new MemoryStream(Dxt5ImageData));
		// 	var x = 0;
		// 	var y = 0;

		// 	var numLogs = 0;

		// 	for (var blockIndex = 0; blockIndex < Dxt5ImageDataLength; blockIndex += 16) {
		// 		var alpha0 = reader.ReadByte();
		// 		var alpha1 = reader.ReadByte();
		// 		var alphaMod = reader.ReadBytes(6); // 48 bits; 4x4 3-bit values
		// 		var color0raw = reader.ReadInt16();
		// 		var color1raw = reader.ReadInt16();
		// 		var colorMod = reader.ReadBytes(4); // 32 bits; 4x4 2-bit values

		// 		var alphaLookup = BitsToInts(alphaMod, 3);
		// 		var colorLookup = BitsToInts(colorMod, 2);
		// 		var alphas = BuildAlphas(alpha0, alpha1);
		// 		var colors = BuildColors(color0raw, color1raw);
		// 		if (numLogs < 1) {
		// 			Console.WriteLine("------------------------");
		// 			Console.WriteLine($"a0: {alpha0}");
		// 			Console.WriteLine($"a1: {alpha1}");
		// 			Console.WriteLine(string.Join("\n", alphas.Select(a => a.ToString())));
		// 			Console.WriteLine("------------------------");
		// 			Console.WriteLine($"c0: {color0raw}");
		// 			Console.WriteLine($"c1: {color1raw}");
		// 			Console.WriteLine(string.Join("\n", colors.Select(c => $"<{c.R} {c.G} {c.B} {c.A}>")));
		// 			Console.WriteLine("------------------------");

		// 			var alphaLookupGroups = alphaLookup
		// 				.Select((n, i) => new { s = n.ToString(), i })
		// 				.GroupBy(g => g.i / 4, o => o.s)
		// 			;
		// 			foreach (var g in alphaLookupGroups) {
		// 				Console.WriteLine(string.Join(" ", g));
		// 			}
		// 			Console.WriteLine("------------------------");
		// 			var colorLookupGroups = colorLookup
		// 				.Select((n, i) => new { s = n.ToString(), i })
		// 				.GroupBy(g => g.i / 4, o => o.s)
		// 			;
		// 			foreach (var g in colorLookupGroups) {
		// 				Console.WriteLine(string.Join(" ", g));
		// 			}
		// 			Console.WriteLine("------------------------");
		// 			numLogs++;
		// 		}
		// 		for (var i = 0; i < 4; i++) {
		// 			for (var j = 0; j < 4; j++) {
		// 				var n = j * 4 + i;
		// 				var a = alphas[alphaLookup[n]];
		// 				var rgb = colors[colorLookup[n]];
		// 				var c = new Color(rgb.R, rgb.G, rgb.B, a);
		// 				var pi = ((y + j) * w) + x + i;
		// 				pixels[pi] = c;
		// 			}
		// 		}

		// 		if (x + 4 < w) {
		// 			x += 4;
		// 		} else {
		// 			y += 4;
		// 			x = 0;
		// 		}
		// 	}

		// 	image.Save(File.OpenWrite(@"C:\output.png"));
		// }

		public void ProcessDxt5ImageData(int width, int height) {
			var image = new Image(width, height);
			var pixels = image.Pixels;
			var reader = new BinaryReader(new MemoryStream(Dxt5ImageData));

			var height_4 = height / 4;
			var width_4 = width / 4;

			for (var by = 0; by < height_4; by++) {
				for (var bx = 0; bx < width_4; bx++) {
					var alphas = BuildAlphas(reader.ReadByte(), reader.ReadByte());
					var alphaIndices = reader.ReadBytes(6); // 48 bits; 4x4 3-bit values
					var colors = BuildColors(reader.ReadInt16(), reader.ReadInt16());
					var colorIndices = reader.ReadBytes(4); // 32 bits; 4x4 2-bit values
					var alphaLookup = BitsToInts(alphaIndices, 3);
					var colorLookup = BitsToInts(colorIndices, 2);

					for (var y = 0; y < 4; y++) {
						for (var x = 0; x < 4; x++) {
							var blockIndex = y * 4 + x;
							var alpha = alphas[alphaLookup[blockIndex]];
							var color = colors[colorLookup[blockIndex]];
							var pixelIndex = (by * 4 + y) * width + bx * 4 + x;
							Debug.Assert(pixelIndex < 512 * 1024, $"Not less than {512 * 1024}, instead is {pixelIndex} x{x} y{y} bx{bx} by{by} w_4{width_4} h_4{height_4}");
							pixels[pixelIndex] = new Color(color.R, color.G, color.B, alpha);
						}
					}
				}
			}

			image.Save(File.OpenWrite(@"C:\output.png"));
		}

		public int[] BitsToInts(byte[] bytes, int bitsPerInt) {
			var bits = new BitArray(bytes);
			var numInts = bytes.Length * 8 / bitsPerInt;
			var ints = new int[numInts];
			var bitsIndex = 0;
			for (var i = 0; i < numInts; i++) {
				var n = 0;
				for (var b = 0; b < bitsPerInt; b++) {
					n <<= 1;
					n |= Convert.ToInt32(bits[bitsIndex]);
					bitsIndex++;
				}
				ints[i] = n;
			}
			return ints;
		}

		public double Lerp(double from, double to, double amount) {
			return from + (to - from) * amount;
		}
		public byte[] BuildAlphas(byte a0, byte a1) {
			var alphas = new byte[8];
			alphas[0] = a0;
			alphas[1] = a1;

			if (a0 > a1) {
				alphas[2] = (byte)Math.Floor(Lerp(a0, a1, 1.0 / 7.0));
				alphas[3] = (byte)Math.Floor(Lerp(a0, a1, 2.0 / 7.0));
				alphas[4] = (byte)Math.Floor(Lerp(a0, a1, 3.0 / 7.0));
				alphas[5] = (byte)Math.Floor(Lerp(a0, a1, 4.0 / 7.0));
				alphas[6] = (byte)Math.Floor(Lerp(a0, a1, 5.0 / 7.0));
				alphas[7] = (byte)Math.Floor(Lerp(a0, a1, 6.0 / 7.0));
			} else {
				alphas[2] = (byte)Math.Floor(Lerp(a0, a1, 1.0 / 5.0));
				alphas[3] = (byte)Math.Floor(Lerp(a0, a1, 2.0 / 5.0));
				alphas[4] = (byte)Math.Floor(Lerp(a0, a1, 3.0 / 5.0));
				alphas[5] = (byte)Math.Floor(Lerp(a0, a1, 4.0 / 5.0));
				alphas[6] = 0;
				alphas[7] = 255;
			}

			// var numAlphasToCompute = 6;
			// if (a0 <= a1) {
			// 	numAlphasToCompute = 4;
			// 	alphas[6] = 0;
			// 	alphas[7] = 255;
			// }
			// int denominator = numAlphasToCompute + 1;
			// var n = 2 + numAlphasToCompute;
			// var a1f = numAlphasToCompute;
			// var a0f = 1;
			// for (var i = 2; i < n; i++) {
			// 	alphas[i] = (byte)((a0f * a0 + a1f * a1) / denominator);
			// 	a0f++;
			// 	a1f--;
			// }
			return alphas;
		}

		public Color[] BuildColors(short color0raw, short color1raw) {
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

		public Color BuildColorFrom565Short(short sColor) {
			var color = (uint)sColor;
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