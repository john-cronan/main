using System;
using System.Linq;
using System.Text;

namespace JC.CommandLine
{
    internal static class ReflectionUtility
    {
        public static object DefaultValue(Type ofType)
        {
            Guard.IsNotNull(ofType, nameof(ofType));

            return ofType.IsValueType 
                ? Activator.CreateInstance(ofType)
                : null;
        }

        public static string GetTypeName(Type t)
        {
            Guard.IsNotNull(t, nameof(t));

            var str = new StringBuilder();
            str.Append(t.Name.Split('`')[0]);
            var args = t.GetGenericArguments().Select(arg => GetTypeName(arg));
            if (args.Any())
            {
                str.Append("<");
                str.Append(string.Join(", ", args));
                str.Append(">");
            }
            return str.ToString();
        }

        public static string GetTypeName(object o)
        {
            Guard.IsNotNull(o, nameof(o));

            return GetTypeName(o.GetType());
        }
    }
}
