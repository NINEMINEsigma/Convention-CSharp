using System;
using System.Collections;
using System.Collections.Generic;
using Convention.EasySave.Internal;
using System.Reflection;
using System.Linq;

namespace Convention.EasySave.Types
{
	public class EasySaveStackType : ECollectionType
	{
		public EasySaveStackType(Type type) : base(type){}

		public override void Write(object obj, EasySaveWriter writer, EasySave.ReferenceMode memberReferenceMode)
		{
			var list = (ICollection)obj;

			if(elementType == null)
				throw new ArgumentNullException("EasySaveType argument cannot be null.");

			//writer.StartWriteCollection();

			int i = 0;
			foreach(object item in list)
			{
				writer.StartWriteCollectionItem(i);
				writer.Write(item, elementType, memberReferenceMode);
				writer.EndWriteCollectionItem(i);
				i++;
			}

			//writer.EndWriteCollection();
		}

		public override object Read<T>(EasySaveReader reader)
		{
            return Read(reader);
			/*if(reader.StartReadCollection())
				return null;

			var stack = new Stack<T>();

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadCollectionItem())
					break;
				stack.CompleteLine(reader.Read<T>(elementType));
				if(reader.EndReadCollectionItem())
					break;
			}

			reader.EndReadCollection();
			return stack;*/
		}

		public override void ReadInto<T>(EasySaveReader reader, object obj)
		{
			if(reader.StartReadCollection())
				throw new NullReferenceException("The Collection we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

			int itemsLoaded = 0;

			var stack = (Stack<T>)obj;

			// Iterate through each item in the collection and try to load it.
			foreach(var item in stack)
			{
				itemsLoaded++;

				if(!reader.StartReadCollectionItem())
					break;

				reader.ReadInto<T>(item, elementType);

				// If we find a ']', we reached the end of the array.
				if(reader.EndReadCollectionItem())
					break;
				// If there's still items to load, but we've reached the end of the collection we're loading into, throw an error.
				if(itemsLoaded == stack.Count)
					throw new IndexOutOfRangeException("The collection we are loading is longer than the collection provided as a parameter.");
			}

			// If we loaded fewer items than the parameter collection, throw index out of range exception.
			if(itemsLoaded != stack.Count)
				throw new IndexOutOfRangeException("The collection we are loading is shorter than the collection provided as a parameter.");

			reader.EndReadCollection();
		}

		public override object Read(EasySaveReader reader)
		{
			var instance = (IList)EasySaveReflection.CreateInstance(EasySaveReflection.MakeGenericType(typeof(List<>), elementType.type));

			if(reader.StartReadCollection())
				return null;

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadCollectionItem())
					break;
				instance.Add(reader.Read<object>(elementType));

				if(reader.EndReadCollectionItem())
					break;
			}

			reader.EndReadCollection();

            EasySaveReflection.GetMethods(instance.GetType(), "Reverse").FirstOrDefault(t => !t.IsStatic).Invoke(instance, new object[]{});
            return EasySaveReflection.CreateInstance(type, instance);
            
		}

		public override void ReadInto(EasySaveReader reader, object obj)
		{
			if(reader.StartReadCollection())
				throw new NullReferenceException("The Collection we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

			int itemsLoaded = 0;

			var collection = (ICollection)obj;

			// Iterate through each item in the collection and try to load it.
			foreach(var item in collection)
			{
				itemsLoaded++;

				if(!reader.StartReadCollectionItem())
					break;

				reader.ReadInto<object>(item, elementType);

				// If we find a ']', we reached the end of the array.
				if(reader.EndReadCollectionItem())
					break;
				// If there's still items to load, but we've reached the end of the collection we're loading into, throw an error.
				if(itemsLoaded == collection.Count)
					throw new IndexOutOfRangeException("The collection we are loading is longer than the collection provided as a parameter.");
			}

			// If we loaded fewer items than the parameter collection, throw index out of range exception.
			if(itemsLoaded != collection.Count)
				throw new IndexOutOfRangeException("The collection we are loading is shorter than the collection provided as a parameter.");

			reader.EndReadCollection();
		}
	}
}