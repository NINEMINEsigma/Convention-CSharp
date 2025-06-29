/*
 * System.Random is no longer serializable at runtime due to Unity changing the implementation away from .NET.
 */

using System;

namespace Convention.EasySave.Types
{
	[EasySaveProperties("inext", "inextp", "SeedArray")]
	public class EasySaveType_Random : EasySaveObjectType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_Random() : base(typeof(System.Random)){ Instance = this; }

		protected override void WriteObject(object obj, EasySaveWriter writer)
		{
			var instance = (System.Random)obj;
			
			writer.WritePrivateField("inext", instance);
			writer.WritePrivateField("inextp", instance);
			writer.WritePrivateField("SeedArray", instance);
		}

		protected override void ReadObject<T>(EasySaveReader reader, object obj)
		{
			var instance = (System.Random)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "inext":
					reader.SetPrivateField("inext", reader.Read<System.Int32>(), instance);
					break;
					case "inextp":
					reader.SetPrivateField("inextp", reader.Read<System.Int32>(), instance);
					break;
					case "SeedArray":
					reader.SetPrivateField("SeedArray", reader.Read<System.Int32[]>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(EasySaveReader reader)
		{
			var instance = new System.Random();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}

	public class EasySaveType_RandomArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public EasySaveType_RandomArray() : base(typeof(System.Random[]), EasySaveType_Random.Instance)
		{
			Instance = this;
		}
	}
}