using System;
using System.Collections;
using System.Collections.Generic;
using Convention.EasySave.Types;

namespace Convention.EasySave.Internal
{
	public static class EasySaveTypeMgr
	{
        private static object _lock = new object();

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public static Dictionary<Type, EasySaveType> types = null;

        // We cache the last accessed type as we quite often use the same type multiple times,
        // so this improves performance as another lookup is not required.
        private static EasySaveType lastAccessedType = null;

		public static EasySaveType GetOrCreateEasySaveType(Type type, bool throwException = true)
		{
			if(types == null)
				Init();

            if (type != typeof(object) && lastAccessedType != null && lastAccessedType.type == type)
                return lastAccessedType;

			// If type doesn't exist, create one.
			if(types.TryGetValue(type, out lastAccessedType))
				return lastAccessedType;
			return (lastAccessedType = CreateES3Type(type, throwException));
		}

		public static EasySaveType GetES3Type(Type type)
		{
			if(types == null)
				Init();

			if(types.TryGetValue(type, out lastAccessedType))
				return lastAccessedType;
			return null;
		}

		internal static void Add(Type type, EasySaveType es3Type)
		{
			if(types == null)
				Init();

            var existingType = GetES3Type(type);
            if (existingType != null && existingType.priority > es3Type.priority)
                return;

            lock (_lock)
            {
                types[type] = es3Type;
            }
		}

		internal static EasySaveType CreateES3Type(Type type, bool throwException = true)
		{
			EasySaveType es3Type;

			if(EasySaveReflection.IsEnum(type))
				return new EasySaveType_enum(type);
			else if(EasySaveReflection.TypeIsArray(type))
			{
				int rank = EasySaveReflection.GetArrayRank(type);
				if(rank == 1)
					es3Type = new EasySaveArrayType(type);
				else if(rank == 2)
					es3Type = new EasySave2DArrayType(type);
				else if(rank == 3)
					es3Type = new EasySave3DArrayType(type);
				else if(throwException)
					throw new NotSupportedException("Only arrays with up to three dimensions are supported by Easy Save.");
				else
					return null;
			}
			else if(EasySaveReflection.IsGenericType(type) && EasySaveReflection.ImplementsInterface(type, typeof(IEnumerable)))
			{
				Type genericType = EasySaveReflection.GetGenericTypeDefinition(type);
                if (typeof(List<>).IsAssignableFrom(genericType))
                    es3Type = new EasySaveListType(type);
                else if (typeof(Dictionary<,>).IsAssignableFrom(genericType))
                    es3Type = new EasySaveDictionaryType(type);
                else if (genericType == typeof(Queue<>))
                    es3Type = new EasySaveQueueType(type);
                else if (genericType == typeof(Stack<>))
                    es3Type = new EasySaveStackType(type);
                else if (genericType == typeof(HashSet<>))
                    es3Type = new EasySaveHashSetType(type);
                else if (genericType == typeof(Unity.Collections.NativeArray<>))
                    es3Type = new EasySaveNativeArrayType(type);
                else if (throwException)
                    throw new NotSupportedException("Generic type \"" + type.ToString() + "\" is not supported by Easy Save.");
                else
                    return null;
			}
			else if(EasySaveReflection.IsPrimitive(type)) // ERROR: We should not have to create an EasySaveType for a primitive.
			{
				if(types == null || types.Count == 0)	// If the type list is not initialised, it is most likely an initialisation error.
					throw new TypeLoadException("EasySaveType for primitive could not be found, and the type list is empty. Please contact Easy Save developers at http://www.moodkie.com/contact");
				else // Else it's a different error, possibly an error in the specific EasySaveType for that type.
					throw new TypeLoadException("EasySaveType for primitive could not be found, but the type list has been initialised and is not empty. Please contact Easy Save developers on mail@moodkie.com");
			}
			else
			{
                if (EasySaveReflection.IsAssignableFrom(typeof(Component), type))
                    es3Type = new ES3ReflectedComponentType(type);
                else if (EasySaveReflection.IsValueType(type))
                    es3Type = new EasySaveReflectedValueType(type);
                else if (EasySaveReflection.IsAssignableFrom(typeof(ScriptableObject), type))
                    es3Type = new ES3ReflectedScriptableObjectType(type);
                else if (EasySaveReflection.IsAssignableFrom(typeof(UnityEngine.Object), type))
                    es3Type = new ES3ReflectedUnityObjectType(type);
                /*else if (EasySaveReflection.HasParameterlessConstructor(type) || EasySaveReflection.IsAbstract(type) || EasySaveReflection.IsInterface(type))
                    es3Type = new EasySaveReflectedObjectType(type);*/
                else if (type.Name.StartsWith("Tuple`"))
                    es3Type = new EasySaveTupleType(type);
                /*else if (throwException)
                    throw new NotSupportedException("Type of " + type + " is not supported as it does not have a parameterless constructor. Only value types, Components or ScriptableObjects are supportable without a parameterless constructor. However, you may be able to create an EasySaveType script to add support for it.");*/
                else
                    es3Type = new EasySaveReflectedObjectType(type);
            }

			if(es3Type.type == null || es3Type.isUnsupported)
			{
				if(throwException)
					throw new NotSupportedException(string.Format("EasySaveType.type is null when trying to create an EasySaveType for {0}, possibly because the element type is not supported.", type));
				return null;
			}

            Add(type, es3Type);
			return es3Type;
		}

        internal static void Init()
        {
            lock (_lock)
            {
                types = new Dictionary<Type, EasySaveType>();
                // Convention.EasySave.Types add themselves to the types Dictionary.
                EasySaveReflection.GetInstances<EasySaveType>();

                // Check that the type list was initialised correctly.
                if (types == null || types.Count == 0)
                    throw new TypeLoadException("Type list could not be initialised. Please contact Easy Save developers on mail@moodkie.com.");
            }
        }
	}
}
