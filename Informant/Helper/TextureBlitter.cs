using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Slothsoft.Informant.Helper;

internal static class TextureBlitter
{
    public static Texture2D Blit(this Texture2D source, Rectangle rectangle, GraphicsDevice? device = null)
    {
        device ??= Game1.graphics.GraphicsDevice;
        var texture = new Texture2D(device, rectangle.Width, rectangle.Height);
        Color[] data = new Color[rectangle.Width * rectangle.Height];
        source.GetData(0, rectangle, data, 0, data.Length);
        texture.SetData(data);
        return texture;
    }
}