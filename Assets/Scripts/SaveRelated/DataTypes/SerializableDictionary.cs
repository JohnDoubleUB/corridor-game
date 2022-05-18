using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SerializableDictionary<TSerializableKey, TSerializableValue>
{
    private _SerializedPair<TSerializableKey, TSerializableValue>[] serializedPairs;

    public Dictionary<TSerializableKey, TSerializableValue> Deserialize()
    {
        Dictionary<TSerializableKey, TSerializableValue> deserializedDict = new Dictionary<TSerializableKey, TSerializableValue>();
        foreach (_SerializedPair<TSerializableKey, TSerializableValue> pair in serializedPairs) deserializedDict.Add(pair.Key, pair.Value);
        return deserializedDict;
    }

    public SerializableDictionary(Dictionary<TSerializableKey, TSerializableValue> DictionaryToSerialize)
    {
        serializedPairs = DictionaryToSerialize.Select(x => new _SerializedPair<TSerializableKey, TSerializableValue>() { Key = x.Key, Value = x.Value }).ToArray();
    }

    [System.Serializable]
    struct _SerializedPair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }
}
