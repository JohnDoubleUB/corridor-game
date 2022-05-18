using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SerializableDictionary<TSerializableKey, TSerializableValue>
{
    public SerializedPair<TSerializableKey, TSerializableValue>[] serializedPairs;

    public SerializableDictionary() 
    {
        serializedPairs = new SerializedPair<TSerializableKey, TSerializableValue>[0];
    }

    public Dictionary<TSerializableKey, TSerializableValue> Deserialize()
    {
        Dictionary<TSerializableKey, TSerializableValue> deserializedDict = new Dictionary<TSerializableKey, TSerializableValue>();
        foreach (SerializedPair<TSerializableKey, TSerializableValue> pair in serializedPairs) deserializedDict.Add(pair.Key, pair.Value);
        return deserializedDict;
    }

    public SerializableDictionary(Dictionary<TSerializableKey, TSerializableValue> DictionaryToSerialize)
    {
        serializedPairs = DictionaryToSerialize.Select(x => new SerializedPair<TSerializableKey, TSerializableValue>() { Key = x.Key, Value = x.Value }).ToArray();
    }
}

[System.Serializable]
public struct SerializedPair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;
}