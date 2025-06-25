using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Convention
{
    public static class PlatformIndicator
    {
#if DEBUG
        public static bool IsRelease => false;
#else
        public static bool IsRelease => true;
#endif
        public static bool IsPlatformWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsPlatformLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsPlatformOsx => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        public static bool IsPlatformX64 => System.Environment.Is64BitOperatingSystem;

        static PlatformIndicator()
        {
            MainThreadID = Thread.CurrentThread.ManagedThreadId;
        }

        public static int MainThreadID { get; private set; }
        public static bool CurrentThreadIsMainThread()
        {
            return MainThreadID == Thread.CurrentThread.ManagedThreadId;
        }
    }

    public static partial class Utility
    {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ConvertString(object obj)
        {
            return Convert.ToString(obj);
        }
        public static T ConvertValue<T>(string str)
        {
            Type type = typeof(T);
            var parse_method = type.GetMethod("Parse");
            if (parse_method != null &&
                (parse_method.ReturnType.IsSubclassOf(type) || parse_method.ReturnType == type) &&
                parse_method.GetParameters().Length == 1 &&
                parse_method.GetParameters()[0].ParameterType == typeof(string))
            {
                return (T)parse_method.Invoke(null, new object[] { str });
            }

            throw new InvalidCastException($"\"{str}\" is cannt convert to type<{type}>");
        }


        public static object SeekValue(object obj, string name, BindingFlags flags, out bool isSucceed)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            isSucceed = true;
            if (field != null)
            {
                return field.GetValue(obj);
            }
            var property = type.GetProperty(name, flags);
            if (property != null)
            {
                return property.GetValue(obj);
            }
            isSucceed = false;
            return null;
        }
        public static object SeekValue(object obj, string name, BindingFlags flags)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            if (field != null)
            {
                return field.GetValue(obj);
            }
            var property = type.GetProperty(name, flags);
            if (property != null)
            {
                return property.GetValue(obj);
            }
            return null;
        }
        public static object SeekValue(object obj, string name, Type valueType, BindingFlags flags, out bool isSucceed)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            isSucceed = true;
            if (field != null && field.FieldType == valueType)
            {
                return field.GetValue(obj);
            }
            var property = type.GetProperty(name, flags);
            if (property != null && property.PropertyType == valueType)
            {
                return property.GetValue(obj);
            }
            isSucceed = false;
            return null;
        }
        public static object SeekValue(object obj, string name, Type valueType, BindingFlags flags)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            if (field != null && field.FieldType == valueType)
            {
                return field.GetValue(obj);
            }
            var property = type.GetProperty(name, flags);
            if (property != null && property.PropertyType == valueType)
            {
                return property.GetValue(obj);
            }
            return null;
        }
        public static bool PushValue(object obj, object value, string name, BindingFlags flags)
        {
            Type type = obj.GetType();
            var field = type.GetField(name, flags);
            if (field != null)
            {
                field.SetValue(obj, value);
                return true;
            }
            var property = type.GetProperty(name, flags);
            if (property != null)
            {
                property.SetValue(obj, value);
                return true;
            }
            return false;
        }

        public static List<Type> SeekType(Predicate<Type> pr, IEnumerable<Assembly> assemblys = null, int findCount = -1)
        {
            List<Type> types = new List<Type>();
            if (assemblys == null)
                assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblys)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (pr(type))
                        types.Add(type);
                    if (types.Count == findCount)
                        return types;
                }
            }
            return types;
        }

        public static List<MemberInfo> GetMemberInfos(Type type, IEnumerable<Type> cutOffType = null, bool isGetNotPublic = false, bool isGetStatic = false)
        {
            Type current = type;
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            List<MemberInfo> result = new();
            if (isGetNotPublic)
                flags |= BindingFlags.NonPublic;
            if (isGetStatic)
                flags |= BindingFlags.Static;
            while ((cutOffType != null && !cutOffType.Contains(current)) && current != null)
            {
                result.AddRange(current.GetFields(flags));
                result.AddRange(current.GetProperties(flags));
                result.AddRange(current.GetMethods(flags));
                current = current.BaseType;
            }
            return result;
        }

        public static bool IsNumber([In] object data)
        {
            if (data == null) return false;
            var type = data.GetType();
            return IsNumber(type);
        }
        public static bool IsString([In] object data)
        {
            if (data == null) return false;
            var type = data.GetType();
            return IsString(type);
        }
        public static bool IsBinary([In] object data)
        {
            if (data == null) return false;
            var type = data.GetType();
            return IsBinary(type);
        }
        public static bool IsArray([In] object data)
        {
            if (data == null) return false;
            var type = data.GetType();
            return IsArray(type);
        }
        public static bool IsBool([In] object data)
        {
            if (data == null) return false;
            return IsBool(data.GetType());
        }

        public static bool IsNumber([In] Type type)
        {
            return
                type == typeof(double) ||
                type == typeof(float) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(uint) ||
                type == typeof(ulong) ||
                type == typeof(char);
        }
        public static bool IsString([In] Type type)
        {
            return type == typeof(string) || type == typeof(char[]);
        }
        public static bool IsBinary([In] Type type)
        {
            return
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(byte[]) ||
                type == typeof(sbyte[]);
        }
        public static bool IsArray([In] Type type)
        {
            return type.IsArray;
        }
        public static bool IsBool([In] Type type)
        {
            return type == typeof(bool);
        }
        public static bool IsEnum([In] Type type)
        {
            return type.IsEnum;
        }

        public static bool HasCustomAttribute(MemberInfo member, IEnumerable<Type> attrs)
        {
            foreach (var attr in attrs)
            {
                if (member.GetCustomAttribute(attr, true) != null)
                    return true;
            }
            return false;
        }
        public static Type GetMemberValueType(MemberInfo member)
        {
            if (member is FieldInfo field)
            {
                return field.FieldType;
            }
            else if (member is PropertyInfo property)
            {
                return property.PropertyType;
            }
            return null;
        }
        public static bool GetMemberValueType(MemberInfo member, out Type type)
        {
            if (member is FieldInfo field)
            {
                type = field.FieldType;
                return true;
            }
            else if (member is PropertyInfo property)
            {
                type = property.PropertyType;
                return true;
            }
            type = null;
            return false;
        }
        public static void PushValue(object target, object value, MemberInfo info)
        {
            if (info is FieldInfo field)
            {
                if (value.GetType().IsSubclassOf(field.FieldType))
                    field.SetValue(target, value);
                else
                {
                    field.SetValue(target, field.FieldType.GetMethod(nameof(float.Parse)).Invoke(target, new object[] { value }));
                }
            }
            else if (info is PropertyInfo property)
            {
                property.SetValue(target, value);
            }
            else
            {
                throw new InvalidOperationException("info is unsupport");
            }
        }
        public static object SeekValue(object target, MemberInfo info)
        {
            if (info is FieldInfo field)
            {
                return field.GetValue(target);
            }
            else if (info is PropertyInfo property)
            {
                return property.GetValue(target);
            }
            else
            {
                throw new InvalidOperationException("info is unsupport");
            }
        }
        public static bool TrySeekValue(object target, MemberInfo info, out object value)
        {
            if (info is FieldInfo field)
            {
                value = field.GetValue(target);
                return true;
            }
            else if (info is PropertyInfo property)
            {
                value = property.GetValue(target);
                return true;
            }
            value = null;
            return false;
        }

        public static List<MemberInfo> SeekMemberInfo(object target, IEnumerable<Type> attrs, IEnumerable<Type> types, Type untilBase = null)
        {
            Type _CurType = target.GetType();
            List<MemberInfo> result = new();
            result.AddRange(_CurType.GetMembers(BindingFlags.Public | BindingFlags.Instance));
            while (_CurType != null && _CurType != typeof(object) && _CurType != untilBase)
            {
                result.AddRange(
                    from info in _CurType.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                    where attrs == null || HasCustomAttribute(info, attrs)
                    where types == null || (GetMemberValueType(info, out var type) && types.Contains(type))
                    select info
                    );
                _CurType = _CurType.BaseType;
            }
            return result;
        }
        public static List<MemberInfo> SeekMemberInfo(object target, IEnumerable<string> names, BindingFlags flags = BindingFlags.Default)
        {
            Type _CurType = target.GetType();
            List<MemberInfo> result = _CurType.GetMembers(flags).ToList();
            HashSet<string> nameSet = names.ToHashSet();
            result.RemoveAll(x => nameSet.Contains(x.Name) == false);
            return result;
        }
        public static object InvokeMember(MemberInfo member, object target, params object[] parameters)
        {
            if (member is MethodInfo method)
            {
                return method.Invoke(target, parameters);
            }
            return null;
        }
        public static bool TryInvokeMember(MemberInfo member, object target, out object returnValue, params object[] parameters)
        {
            returnValue = null;
            if (member is MethodInfo method)
            {
                returnValue = method.Invoke(target, parameters);
                return true;
            }
            else return false;
        }

        public static T Shared<T>(T target, out T value)
        {
            value = target;
            return value;
        }


        public static string NowFormat(string format = "yyyy-MM-dd_HH-mm-ss")
        {
            return DateTime.Now.ToString(format);
        }
    }
}
