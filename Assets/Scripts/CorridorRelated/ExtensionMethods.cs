using System;
using System.Collections.Generic;
using System.Linq;
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
                //Debug.Log("sprite x: " + sprite.textureRect.x);

                Color[] newColors = sprite.texture.GetPixels((int)System.Math.Ceiling(sprite.rect.x),
                                                             (int)System.Math.Ceiling(sprite.rect.y),
                                                             (int)System.Math.Ceiling(sprite.rect.width),
                                                             (int)System.Math.Ceiling(sprite.rect.height));
                //Debug.Log(colors.Length + "_" + newColors.Length);
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

    public static void PlayClipAtTransform(this Transform transform, AudioClip clip, bool parentToTransform = true, float volume = 1f, bool withPitchVariation = true, float delayInSeconds = 0f) 
    {
        if (AudioManager.current != null) 
        {
            AudioSource audio = AudioManager.current.PlayClipAt(clip, transform.position, volume, withPitchVariation, delayInSeconds);
            if (parentToTransform) audio.transform.parent = transform;
        }
    }

    public static IEnumerable<IEnumerable<TSource>> Partition<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        List<TSource> result1 = new List<TSource>();
        List<TSource> result2 = new List<TSource>();

        foreach (TSource sourceItem in source) 
        {
            if (predicate(sourceItem))
            {
                result1.Add(sourceItem);
            }
            else 
            {
                result2.Add(sourceItem);
            }
        }

        yield return result1;
        yield return result2;
    }

    public static bool AnyAndAllMatchPredicate<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) 
    {
        bool hasItems = false;

        foreach (TSource sourceItem in source) 
        {
            hasItems = true;
            
            if (!predicate(sourceItem)) 
            {
                return false;
            }
        }

        return hasItems;
    }


    public static bool AnyAndAnyMatchPredicate<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {

        foreach (TSource sourceItem in source)
        {
            if (predicate(sourceItem))
            {
                return true;
            }
        }

        return false;
    }
}
