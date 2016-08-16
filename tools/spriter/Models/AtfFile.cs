using System;
using System.IO;
using System.Text;
using ImageProcessorCore;
using Spriter.Extensions;

namespace Spriter.Models
{
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
			foreach (var data in textureData) {
				builder.AppendLine(data.ToString());
				builder.AppendLine($"=========================================");
			}

			return builder.ToString();
		}

		public void SaveAsPng(string path) {
			foreach (var data in textureData) {
				var arc = data as AtfRawCompressed;
				var arca = data as AtfRawCompressedAlpha;
				if (arc != null && arc.Dxt1ImageDataLength > 0) {
					arc.SaveDxt1ImageDataAsPng(Width, Height, path);
				}
				if (arca != null && arca.Dxt5ImageDataLength > 0) {
					arca.SaveDxt5ImageDataAsPng(Width, Height, path);
				}
			}
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
				case AtfFormat.ATFRAWCOMPRESSED: return AtfRawCompressed.FromBytes(reader, count * (file.IsCubemap ? 6 : 1));
				case AtfFormat.ATFRAWCOMPRESSEDALPHA: return AtfRawCompressedAlpha.FromBytes(reader, count * (file.IsCubemap ? 6 : 1));
			}
			throw new NotSupportedException($"Cannot build texture data for format {file.Format.GetDisplayDescription()}");
		}

		public static Color[] BuildColors(UInt16 color0raw, UInt16 color1raw) {
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

		public static Color BuildColorFrom565Short(UInt16 color) {
			var r = (color & 0xF800) >> 11;
			var g = (color & 0x07E0) >> 5;
			var b = (color & 0x001F) >> 0;
			return new Color(r / 31.0f, g / 63.0f, b / 31.0f);
		}
	}
}