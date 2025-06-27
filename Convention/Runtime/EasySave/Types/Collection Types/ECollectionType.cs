using System;
using System.Collections;
using System.Collections.Generic;
using Convention.EasySave.Internal;

namespace Convention.EasySave.Types
{
	public abstract class ECollectionType : EasySaveType
	{
		public EasySaveType elementType;

		/*protected EasySaveReflection.EasySaveReflectedMethod readMethod = null;
		protected EasySaveReflection.EasySaveReflectedMethod readIntoMethod = null;*/

        public abstract object Read(EasySaveReader reader);
        public abstract void ReadInto(EasySaveReader reader, object obj);
        public abstract void Write(object obj, EasySaveWriter writer, EasySave.ReferenceMode memberReferenceMode);

        public ECollectionType(Type type) : base(type)
		{
			elementType = EasySaveTypeMgr.GetOrCreateEasySaveType(EasySaveReflection.GetElementTypes(type)[0], false);
			isCollection = true;

			// If the element type is null (i.e. unsupported), make this EasySaveType null.
			if(elementType == null)
				isUnsupported = true;
		}

        public ECollectionType(Type type, EasySaveType elementType) : base(type)
		{
			this.elementType = elementType;
			isCollection = true;
		}

        public override void Write(object obj, EasySaveWriter writer)
		{
			Write(obj, writer, EasySave.ReferenceMode.ByRefAndValue);
		}

        protected virtual bool ReadICollection<T>(EasySaveReader reader, ICollection<T> collection, EasySaveType elementType)
		{
			if(reader.StartReadCollection())
				return false;

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadCollectionItem())
					break;
				collection.Add(reader.Read<T>(elementType));

				if(reader.EndReadCollectionItem())
					break;
			}

			reader.EndReadCollection();

			return true;
		}

        protected virtual void ReadICollectionInto<T>(EasySaveReader reader, ICollection<T> collection, EasySaveType elementType)
        {
            ReadICollectionInto(reader, collection, elementType);
        }

        protected virtual void ReadICollectionInto(EasySaveReader reader, ICollection collection, EasySaveType elementType)
		{
			if(reader.StartReadCollection())
				throw new NullReferenceException("The Collection we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

			int itemsLoaded = 0;

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

		/*
		 * 	Calls the Read method using reflection so we don't need to provide a generic parameter.
		 */
		/*public virtual object Read(EasySaveReader reader)
		{
			if(readMethod == null)
				readMethod = EasySaveReflection.GetMethod(this.GetType(), "Read", new Type[]{elementType.type}, new Type[]{typeof(EasySaveReader)});
			return readMethod.Invoke(this, new object[]{reader});
		}

		public virtual void ReadInto(EasySaveReader reader, object obj)
		{
			if(readIntoMethod == null)
				readIntoMethod = EasySaveReflection.GetMethod(this.GetType(), "ReadInto", new Type[]{elementType.type}, new Type[]{typeof(EasySaveReader), typeof(object)});
			readIntoMethod.Invoke(this, new object[]{reader, obj});
		}*/
	}
}