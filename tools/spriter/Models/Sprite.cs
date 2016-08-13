using System.Xml.Linq;

namespace Spriter {
	public class Sprite {
		public string TextureAtlasPath { get; private set; }
		public string ImagePath { get; private set; }
		public string Name { get; private set; }
		/// Whether or not the sprite is rotated 90° clockwise
		public bool Rotated { get; private set; }
		/// Where the sprite non-transparent pixel data begins in the X axis
		public int X { get; private set; }
		/// Where the sprite non-transparent pixel data begins in the Y axis
		public int Y { get; private set; }
		/// How wide the sprite non-transparent pixel data is
		public int Width { get; private set; }
		/// How tall the sprite non-transparent pixel data is
		public int Height { get; private set; }
		/// How many horizontal transparent pixels should be added to the non-transparent pixel data; typically negative to represent "pad transparent pixels on the left"
		public int FrameX { get; private set; }
		/// How many vertical transparent pixels should be added to the non-transparent pixel data; typically negative to represent "pad transparent pixels on the top"
		public int FrameY { get; private set; }
		/// How wide was the original sprite?
		public int FrameWidth { get; private set; }
		/// How tall was the original sprite?
		public int FrameHeight { get; private set; }

		private static int IntAttr(XElement elem, string attr) => int.Parse(elem.Attribute(attr)?.Value ?? "0");
		public static Sprite FromXElement(string textureAtlasPath, string imagePath, XElement elem) {
			// Example:
			// <TextureAtlas imagePath="LargeRunes.png">
			//     <SubTexture name="Rune00" x="990" y="1402" rotated="true" width="194" height="200" frameX="-45" frameY="-51" frameWidth="290" frameHeight="290"/>
			//     <SubTexture name="Rune01" x="990" y="1200" rotated="true" width="194" height="200" frameX="-45" frameY="-51" frameWidth="290" frameHeight="290"/>
			//     <SubTexture name="Rune02" x="794" y="1402" rotated="true" width="194" height="200" frameX="-45" frameY="-51" frameWidth="290" frameHeight="290"/>
			//     <SubTexture name="Rune03" x="794" y="1200" rotated="true" width="194" height="200" frameX="-45" frameY="-51" frameWidth="290" frameHeight="290"/>
			//     <SubTexture name="Rune04" x="794" y="726" width="200" height="194" frameX="-45" frameY="-51" frameWidth="290" frameHeight="290"/>
			//     <!-- ... -->
			// </TextureAtlas>
			var sprite = new Sprite();
			sprite.TextureAtlasPath = textureAtlasPath;
			sprite.ImagePath = imagePath;
			sprite.Name = elem.Attribute("name").Value;
			sprite.Rotated = elem.Attribute("rotated") != null;
			sprite.X = IntAttr(elem, "x");
			sprite.Y = IntAttr(elem, "y");
			sprite.Width = IntAttr(elem, "width");
			sprite.Height = IntAttr(elem, "height");
			sprite.FrameX = IntAttr(elem, "frameX");
			sprite.FrameY = IntAttr(elem, "frameY");
			sprite.FrameWidth = IntAttr(elem, "frameWidth");
			sprite.FrameHeight = IntAttr(elem, "frameHeight");
			return sprite;
		}
	}
}