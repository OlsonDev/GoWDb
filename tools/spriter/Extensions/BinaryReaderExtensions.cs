using System;
using System.IO;

namespace Spriter.Extensions {
	public static class BinaryReaderExtensions {
		public static byte[] ReadAllBytes(this BinaryReader reader) {
				const int bufferSize = 4096;
				using (var ms = new MemoryStream()) {
						var buffer = new byte[bufferSize];
						int count;
						while ((count = reader.Read(buffer, 0, buffer.Length)) != 0) {
							ms.Write(buffer, 0, count);
						}
						return ms.ToArray();
				}
		}

  public static byte[] ReadBytesRequired(this BinaryReader reader, int count) {
    var bytes = reader.ReadBytes(count);
    if (bytes.Length != count) {
			throw new EndOfStreamException($"{count} bytes required from stream, but only {bytes.Length} returned.");
		}
    return bytes;
  }

		/// Reads a Big Endian Int16
		public static Int16 ReadInt16BE(this BinaryReader reader) => BitConverter.ToInt16(reader.ReadBytesRequired(2).Reverse(), 0);
		/// Reads a Big Endian Int32
		public static Int32 ReadInt32BE(this BinaryReader reader) => BitConverter.ToInt32(reader.ReadBytesRequired(4).Reverse(), 0);
		/// Reads a Big Endian Int64
		public static Int64 ReadInt64BE(this BinaryReader reader) => BitConverter.ToInt64(reader.ReadBytesRequired(8).Reverse(), 0);
		/// Reads a Big Endian UInt16
		public static UInt16 ReadUInt16BE(this BinaryReader reader) => BitConverter.ToUInt16(reader.ReadBytesRequired(2).Reverse(), 0);
		/// Reads a Big Endian UInt32
		public static UInt32 ReadUInt32BE(this BinaryReader reader) => BitConverter.ToUInt32(reader.ReadBytesRequired(4).Reverse(), 0);
		/// Reads a Big Endian UInt64
		public static UInt64 ReadUInt64BE(this BinaryReader reader) => BitConverter.ToUInt64(reader.ReadBytesRequired(8).Reverse(), 0);

	}
}