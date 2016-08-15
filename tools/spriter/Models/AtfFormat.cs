using System.ComponentModel.DataAnnotations;

namespace Spriter.Models {
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
}