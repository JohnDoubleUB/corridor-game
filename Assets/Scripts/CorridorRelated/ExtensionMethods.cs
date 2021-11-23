using UnityEngine;

public static class ExtensionMethods
{

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static Texture2D ConvertSpriteToTexture(this Sprite sprite)
    {
        try
        {
            if (sprite.rect.width != sprite.texture.width)
            {
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] colors = newText.GetPixels();
                Debug.Log("sprite x: " + sprite.textureRect.x);

                Color[] newColors = sprite.texture.GetPixels((int)System.Math.Ceiling(sprite.rect.x),
                                                             (int)System.Math.Ceiling(sprite.rect.y),
                                                             (int)System.Math.Ceiling(sprite.rect.width),
                                                             (int)System.Math.Ceiling(sprite.rect.height));
                Debug.Log(colors.Length + "_" + newColors.Length);
                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            }
            else
                return sprite.texture;
        }
        catch
        {
            return sprite.texture;
        }
    }

}
