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

    public static AudioSource PlayClipAtTransform(this Transform transform, AudioClip clip, bool parentToTransform = true, float volume = 1f, bool withPitchVariation = true, float delayInSeconds = 0f, bool noiseCanBeHeardByEntities = true, float noiseAlertRadius = 10f)
    {
        if (AudioManager.current != null)
        {
            AudioSource audio = AudioManager.current.PlayClipAt(clip, transform.position, volume, withPitchVariation, delayInSeconds, noiseCanBeHeardByEntities, noiseAlertRadius);
            if (parentToTransform) audio.transform.parent = transform;

            return audio;
        }

        return null;
    }

    public static void GenerateNoiseAlertAtTransform(this Transform transform, float noiseAlertRadius = 10f, NoiseOrigin noiseOrigin = NoiseOrigin.Unspecified)
    {
        if (AudioManager.current != null) AudioManager.current.GenerateNoiseAlert(transform.position, noiseAlertRadius, noiseOrigin);
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

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.Shuffle(new System.Random());
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, System.Random rng)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (rng == null) throw new ArgumentNullException(nameof(rng));

        return source.ShuffleIterator(rng);
    }

    private static IEnumerable<T> ShuffleIterator<T>(
        this IEnumerable<T> source, System.Random rng)
    {
        var buffer = source.ToList();
        for (int i = 0; i < buffer.Count; i++)
        {
            int j = rng.Next(i, buffer.Count);
            yield return buffer[j];

            buffer[j] = buffer[i];
        }
    }

    public static string Truncate(this string variable, int Length)
    {
        if (string.IsNullOrEmpty(variable)) return variable;
        return variable.Length <= Length ? variable : variable.Substring(0, Length);
    }

    public static Vector3 ToXZ(this Vector2 aVec)
    {
        return new Vector3(aVec.x, 0, aVec.y);
    }
    public static Vector3 ToXZ(this Vector2 aVec, float aYValue)
    {
        return new Vector3(aVec.x, aYValue, aVec.y);
    }


    public static void LaunchAtTarget(this Rigidbody rb, Vector3 target, float magnitude = 20f)
    {
        rb.LaunchAtTarget(target, Vector3.zero, magnitude);
    }

    public static void LaunchAtTarget(this Rigidbody rb, Vector3 target, Vector3 spin, float magnitude = 20f)
    {
        float distance = Vector3.Distance(target, rb.position);
        float desiredFinalMagnitude = magnitude * Mathf.Min(distance, 1f);
        float speedNeeded = distance / 4 + desiredFinalMagnitude; // 4 is an arbitrary value which suits well my friction / drag
        Vector3 direction = (target - rb.position).normalized;
        Vector3 result = direction * speedNeeded;
        rb.velocity = result;
        rb.angularVelocity = spin;
    }


    public static float GetAngleToTargetFromSelectedTransform(Transform selectedTransform, Vector3 targetPosition, Vector3 selectedTransformDirection)
    {
        return Mathf.Abs(Vector3.Angle(selectedTransform.forward, new Vector3(targetPosition.x, selectedTransform.position.y, targetPosition.z) - selectedTransform.position));
    }

    public static float GetAngleToTargetFromSelectedTransform(Transform selectedTransform, Vector3 targetPosition)
    {
        return GetAngleToTargetFromSelectedTransform(selectedTransform, targetPosition, selectedTransform.forward);
    }

    public static float GetAngleToTarget(this Transform selectedTransform, Vector3 targetPosition) 
    {
        return GetAngleToTargetFromSelectedTransform(selectedTransform, targetPosition);
    }

    public static float GetAngleToTarget(this Transform selectedTransform, Vector3 targetPosition, Vector3 selectedTransformDirection) 
    {
        return GetAngleToTargetFromSelectedTransform(selectedTransform, targetPosition, selectedTransformDirection);
    }
}
