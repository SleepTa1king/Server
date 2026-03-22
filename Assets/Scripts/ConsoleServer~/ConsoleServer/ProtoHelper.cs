using System.IO;
using ProtoBuf;

/// <summary>
/// Protobuf 序列化工具
/// </summary>
public static class ProtoHelper
{
    /// <summary>
    /// 对象 → byte[]
    /// </summary>
    public static byte[] Serialize<T>(T obj)
    {
        using (var ms = new MemoryStream())
        {
            Serializer.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    /// <summary>
    /// byte[] → 对象
    /// </summary>
    public static T Deserialize<T>(byte[] data)
    {
        using (var ms = new MemoryStream(data))
        {
            return Serializer.Deserialize<T>(ms);
        }
    }
}