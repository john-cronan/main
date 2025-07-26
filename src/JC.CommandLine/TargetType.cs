using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace JC.CommandLine
{
    internal class TargetType
    {
        private readonly bool _isNullable;
        private readonly bool _isVectorType;
        private readonly Type _scalarType;
        private readonly Type _target;

        public TargetType(Type target)
        {
            Guard.IsNotNull(target, nameof(target));

            _target = target;
            _isVectorType = GetIsVectorType(_target);
            _scalarType = GetScalarType(_target, _isVectorType);
            _isNullable = GetIsNullable(target);
        }

        private static Type GetScalarType(Type type, bool isVectorType)
        {
            return isVectorType
                ? GetScalarTypeForVectorType(type)
                : GetScalarTypeForNonVectorType(type);
        }

        private static Type GetScalarTypeForVectorType(Type type)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments();
                if (genericArguments.Length > 1)
                {
                    throw new NotSupportedException("Types with multiple type arguments are not supported");
                }
                return GetScalarTypeForNonVectorType(genericArguments[0]);
            }
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType)
                {
                    var genericInterface = iface.GetGenericTypeDefinition();
                    if (genericInterface == typeof(IEnumerable<>))
                    {
                        return iface.GetGenericArguments()[0];
                    }
                }
            }
            return typeof(object);
        }

        private static Type GetScalarTypeForNonVectorType(Type type)
        {
            if (GetIsNullable(type))
            {
                return type.GetGenericArguments()[0];
            }
            else
            {
                return type;
            }
        }

        private static bool GetIsNullable(Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
            return false;
        }

        private static bool GetIsVectorType(Type type)
        {
            if (type == typeof(string))
            {
                return false;
            }
            if (type == typeof(XmlDocument))
            {
                return false;
            }
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is TargetType)
            {
                return this == (TargetType)obj;
            }
            else if (obj is Type)
            {
                return this == (Type)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _target.GetHashCode();
        }

        public override string ToString()
        {
            return ReflectionUtility.GetTypeName(_target);
        }


        public object DefaultScalarValue => ReflectionUtility.DefaultValue(_scalarType);

        public bool IsBoolean => _scalarType == typeof(bool);

        public bool IsVectorType => _isVectorType;

        public Type Target => _target;

        public Type ScalarType => _scalarType;

        public bool IsNullable => _isNullable;


        public static implicit operator TargetType(Type t)
        {
            return new TargetType(t);
        }

        public static bool operator ==(TargetType x, TargetType y)
        {
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
            {
                return object.ReferenceEquals(x, null) && object.ReferenceEquals(y, null);
            }
            return x.Target == y.Target;
        }

        public static bool operator !=(TargetType x, TargetType y)
        {
            return !(x == y);
        }

        public static bool operator ==(TargetType x, Type y)
        {
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
            {
                return object.ReferenceEquals(x, null) && object.ReferenceEquals(y, null);
            }
            return x.Target == y;
        }

        public static bool operator !=(TargetType x, Type y)
        {
            return !(x == y);
        }

        public static bool operator ==(Type x, TargetType y)
        {
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
            {
                return object.ReferenceEquals(x, null) && object.ReferenceEquals(y, null);
            }
            return x == y.Target;
        }

        public static bool operator !=(Type x, TargetType y)
        {
            return !(x == y);
        }
    }
}
