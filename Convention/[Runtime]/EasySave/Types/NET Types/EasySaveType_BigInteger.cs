using System.Numerics;

namespace Convention.EasySave.Types
{
    [EasySaveProperties("bytes")]
    public class EasySaveType_BigInteger : EasySaveType
    {
        public static EasySaveType Instance = null;

        public EasySaveType_BigInteger() : base(typeof(BigInteger))
        {
            Instance = this;
        }

        public override void Write(object obj, EasySaveWriter writer)
        {
            BigInteger casted = (BigInteger)obj;
            writer.WriteProperty("bytes", casted.ToByteArray(), EasySaveType_byteArray.Instance);
        }

        public override object Read<T>(EasySaveReader reader)
        {
            return new BigInteger(reader.ReadProperty<byte[]>(EasySaveType_byteArray.Instance));
        }
    }

    public class EasySaveType_BigIntegerArray : EasySaveArrayType
    {
        public static EasySaveType Instance;

        public EasySaveType_BigIntegerArray() : base(typeof(BigInteger[]), EasySaveType_BigInteger.Instance)
        {
            Instance = this;
        }
    }
}