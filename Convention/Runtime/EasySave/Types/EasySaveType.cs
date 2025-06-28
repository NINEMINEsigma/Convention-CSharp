using System;
using System.Collections;
using System.Collections.Generic;
using Convention.EasySave.Internal;
using System.Linq;

namespace Convention.EasySave.Types
{
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public abstract class EasySaveType
	{
		public const string typeFieldName = "__type";

		public EasySaveMember[] members;
		public Type type;
		public bool isPrimitive = false;
		public bool isValueType = false;
		public bool isCollection = false;
		public bool isDictionary = false;
        public bool isTuple = false;
        public bool isEnum = false;
		public bool isReflectedType = false;
		public bool isUnsupported = false;
        public int priority = 0;

		protected EasySaveType(Type type)
		{
			EasySaveTypeMgr.Add(type, this);
			this.type = type;
			this.isValueType = EasySaveReflection.IsValueType(type);
		}

		public abstract void Write(object obj, EasySaveWriter writer);
		public abstract object Read<T>(EasySaveReader reader);

		public virtual void ReadInto<T>(EasySaveReader reader, object obj)
		{
			throw new NotImplementedException("Self-assigning Read is not implemented or supported on this type.");
		}

		protected bool WriteUsingDerivedType(object obj, EasySaveWriter writer)
		{
			var objType = obj.GetType();
				
			if(objType != this.type)
			{
				writer.WriteType(objType);
				EasySaveTypeMgr.GetOrCreateEasySaveType(objType).Write(obj, writer);
				return true;
			}
			return false;
		}

		protected void ReadUsingDerivedType<T>(EasySaveReader reader, object obj)
		{
			EasySaveTypeMgr.GetOrCreateEasySaveType(reader.ReadType()).ReadInto<T>(reader, obj);
		}

		internal string ReadPropertyName(EasySaveReader reader)
		{
			if(reader.overridePropertiesName != null)
			{
				string propertyName = reader.overridePropertiesName;
				reader.overridePropertiesName = null;
				return propertyName;
			}
			return reader.ReadPropertyName();
		}
	
		#region Reflection Methods

		protected void WriteProperties(object obj, EasySaveWriter writer)
		{
			if(members == null)
				GetMembers(writer.settings.safeReflection);

			for(int i=0; i<members.Length; i++)
			{
				var property = members[i];
				writer.WriteProperty(property.name, property.reflectedMember.GetValue(obj), EasySaveTypeMgr.GetOrCreateEasySaveType(property.type), writer.settings.memberReferenceMode);
			}
		}

		protected object ReadProperties(EasySaveReader reader, object obj)
		{
            // Iterate through each property in the file and try to load it using the appropriate
            // EasySaveMember in the members array.
            foreach (string propertyName in reader.Properties)
			{
				// Find the property.
				EasySaveMember property = null;
				for(int i=0; i<members.Length; i++)
				{
					if(members[i].name == propertyName)
					{
						property = members[i];
						break;
					}
				}

                // If this is a class which derives directly from a Collection, we need to load it's dictionary first.
                if(propertyName == "_Values")
                {
                    var baseType = EasySaveTypeMgr.GetOrCreateEasySaveType(EasySaveReflection.BaseType(obj.GetType()));
                    if(baseType.isDictionary)
                    {
                        var dict = (IDictionary)obj;
                        var loaded = (IDictionary)baseType.Read<IDictionary>(reader);
                        foreach (DictionaryEntry kvp in loaded)
                            dict[kvp.Key] = kvp.Value;
                    }
                    else if(baseType.isCollection)
                    {
                        var loaded = (IEnumerable)baseType.Read<IEnumerable>(reader);

                        var type = baseType.GetType();

                        if (type == typeof(EasySaveListType))
                            foreach (var item in loaded)
                                ((IList)obj).Add(item);
                        else if (type == typeof(EasySaveQueueType))
                        {
                            var method = baseType.type.GetMethod("Enqueue");
                            foreach (var item in loaded)
                                method.Invoke(obj, new object[] { item });
                        }
                        else if (type == typeof(EasySaveStackType))
                        {
                            var method = baseType.type.GetMethod("Push");
                            foreach (var item in loaded)
                                method.Invoke(obj, new object[] { item });
                        }
                        else if (type == typeof(EasySaveHashSetType))
                        {
                            var method = baseType.type.GetMethod("Add");
                            foreach (var item in loaded)
                                method.Invoke(obj, new object[] { item });
                        }
                    }
                }

                if (property == null)
					reader.Skip();
				else
				{
					var type = EasySaveTypeMgr.GetOrCreateEasySaveType(property.type);

					if(EasySaveReflection.IsAssignableFrom(typeof(EasySaveDictionaryType), type.GetType()))
						property.reflectedMember.SetValue(obj, ((EasySaveDictionaryType)type).Read(reader));
					else if(EasySaveReflection.IsAssignableFrom(typeof(ECollectionType), type.GetType()))
						property.reflectedMember.SetValue(obj, ((ECollectionType)type).Read(reader));
					else
					{
						object readObj = reader.Read<object>(type);
						property.reflectedMember.SetValue(obj, readObj);
					}
				}
			}
			return obj;
		}

		protected void GetMembers(bool safe)
		{
			GetMembers(safe, null);
		}

		protected void GetMembers(bool safe, string[] memberNames)
		{
			var serializedMembers = EasySaveReflection.GetSerializableMembers(type, safe, memberNames);

			members = new EasySaveMember[serializedMembers.Length];
			for(int i=0; i<serializedMembers.Length; i++)
				members[i] = new EasySaveMember(serializedMembers[i]);
		}

		#endregion

	}

	[AttributeUsage(AttributeTargets.Class)]
	public class EasySavePropertiesAttribute : System.Attribute 
	{
		public readonly string[] members;

		public EasySavePropertiesAttribute(params string[] members)
		{
			this.members = members;
		}
	}
}
