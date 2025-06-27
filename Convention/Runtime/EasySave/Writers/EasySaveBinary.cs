using System.Collections;
using System.Collections.Generic;
using System;

namespace Convention.EasySave.Internal
{
    internal enum EasySaveSpecialByte : byte
    {
        Null        = 0,
        Bool        = 1,
        Byte        = 2,
        Sbyte       = 3,
        Char        = 4,
        Decimal     = 5,
        Double      = 6,
        Float       = 7,
        Int         = 8,
        Uint        = 9,
        Long        = 10,
        Ulong       = 11,
        Short       = 12,
        Ushort      = 13,
        String      = 14,
        ByteArray   = 15,
        Collection  = 128,
        Dictionary  = 129,
        CollectionItem = 130,
        Object      = 254,
        Terminator  = 255
    }

    internal static class EasySaveBinary
    {
        internal const string ObjectTerminator = ".";

        internal static readonly Dictionary<EasySaveSpecialByte, Type> IdToType = new Dictionary<EasySaveSpecialByte, Type>()
        {
            { EasySaveSpecialByte.Null,       null },
            { EasySaveSpecialByte.Bool,       typeof(bool)},
            { EasySaveSpecialByte.Byte,       typeof(byte)},
            { EasySaveSpecialByte.Sbyte,      typeof(sbyte)},
            { EasySaveSpecialByte.Char,       typeof(char)},
            { EasySaveSpecialByte.Decimal,    typeof(decimal)},
            { EasySaveSpecialByte.Double,     typeof(double)},
            { EasySaveSpecialByte.Float,      typeof(float)},
            { EasySaveSpecialByte.Int,        typeof(int)},
            { EasySaveSpecialByte.Uint,       typeof(uint)},
            { EasySaveSpecialByte.Long,       typeof(long)},
            { EasySaveSpecialByte.Ulong,      typeof(ulong)},
            { EasySaveSpecialByte.Short,      typeof(short)},
            { EasySaveSpecialByte.Ushort,     typeof(ushort)},
            { EasySaveSpecialByte.String,     typeof(string)},
            { EasySaveSpecialByte.ByteArray,  typeof(byte[])}
        };

        internal static readonly Dictionary<Type, EasySaveSpecialByte> TypeToId = new Dictionary<Type, EasySaveSpecialByte>()
        {
            { typeof(bool),     EasySaveSpecialByte.Bool},
            { typeof(byte),     EasySaveSpecialByte.Byte},
            { typeof(sbyte),    EasySaveSpecialByte.Sbyte},
            { typeof(char),     EasySaveSpecialByte.Char},
            { typeof(decimal),  EasySaveSpecialByte.Decimal},
            { typeof(double),   EasySaveSpecialByte.Double},
            { typeof(float),    EasySaveSpecialByte.Float},
            { typeof(int),      EasySaveSpecialByte.Int},
            { typeof(uint),     EasySaveSpecialByte.Uint},
            { typeof(long),     EasySaveSpecialByte.Long},
            { typeof(ulong),    EasySaveSpecialByte.Ulong},
            { typeof(short),    EasySaveSpecialByte.Short},
            { typeof(ushort),   EasySaveSpecialByte.Ushort},
            { typeof(string),   EasySaveSpecialByte.String},
            { typeof(byte[]),   EasySaveSpecialByte.ByteArray}
        };

        internal static EasySaveSpecialByte TypeToByte(Type type)
        {
            EasySaveSpecialByte b;
            if (TypeToId.TryGetValue(type, out b))
                return b;
            return EasySaveSpecialByte.Object;
        }

        internal static Type ByteToType(EasySaveSpecialByte b)
        {
            return ByteToType((byte)b);
        }

        internal static Type ByteToType(byte b)
        {
            Type type;
            if (IdToType.TryGetValue((EasySaveSpecialByte)b, out type))
                return type;
            return typeof(object);
        }

        internal static bool IsPrimitive(EasySaveSpecialByte b)
        {
            switch(b)
            {
                case EasySaveSpecialByte.Bool:
                case EasySaveSpecialByte.Byte:
                case EasySaveSpecialByte.Sbyte:
                case EasySaveSpecialByte.Char:
                case EasySaveSpecialByte.Decimal:
                case EasySaveSpecialByte.Double:
                case EasySaveSpecialByte.Float:
                case EasySaveSpecialByte.Int:
                case EasySaveSpecialByte.Uint:
                case EasySaveSpecialByte.Long:
                case EasySaveSpecialByte.Ulong:
                case EasySaveSpecialByte.Short:
                case EasySaveSpecialByte.Ushort:
                case EasySaveSpecialByte.String:
                case EasySaveSpecialByte.ByteArray:
                    return true;
                default:
                    return false;
            }
        }
    }
}
