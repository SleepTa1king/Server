using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XLua;

namespace Main.Lua
{
    /// <summary>
    /// 项目自己的 xLua 生成配置。
    /// 第一版只保留最小常用类型，够你先跑通 Lua <-> C#。
    /// </summary>
    public static class GenConfig
    {
        [LuaCallCSharp]
        public static List<Type> LuaCallCSharp = new List<Type>
        {
            typeof(Debug),
            typeof(GameObject),
            typeof(Transform),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Quaternion),
            typeof(Time),
            typeof(Application),
            typeof(UnityEngine.Object)
        };

        [CSharpCallLua]
        public static List<Type> CSharpCallLua = new List<Type>
        {
            typeof(Action),
            typeof(Action<string>)
        };

        [BlackList]
        public static Func<MemberInfo, bool> MethodFilter = memberInfo =>
        {
            if (memberInfo == null)
            {
                return false;
            }

            switch (memberInfo)
            {
                case MethodInfo methodInfo:
                    if (ContainsUnsupportedByRefLikeType(methodInfo.ReturnType))
                    {
                        return true;
                    }

                    return methodInfo.GetParameters().Any(parameter => ContainsUnsupportedByRefLikeType(parameter.ParameterType));

                case ConstructorInfo constructorInfo:
                    return constructorInfo.GetParameters().Any(parameter => ContainsUnsupportedByRefLikeType(parameter.ParameterType));

                case PropertyInfo propertyInfo:
                    return ContainsUnsupportedByRefLikeType(propertyInfo.PropertyType);

                case FieldInfo fieldInfo:
                    return ContainsUnsupportedByRefLikeType(fieldInfo.FieldType);

                default:
                    return false;
            }
        };

        private static bool ContainsUnsupportedByRefLikeType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (type.IsByRef)
            {
                type = type.GetElementType();
            }

            if (type == null)
            {
                return false;
            }

            if (type.IsByRefLike)
            {
                return true;
            }

            if (type.IsArray)
            {
                return ContainsUnsupportedByRefLikeType(type.GetElementType());
            }

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Span<>) ||
                    type.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>))
                {
                    return true;
                }

                return type.GetGenericArguments().Any(ContainsUnsupportedByRefLikeType);
            }

            return false;
        }
    }
}
