using System;
using System.Linq;
using System.Reflection;

namespace Tinkoff.ISA.Infrastructure.Utils
{
    public static class MemberInfoExtensions
    {
        public static bool HasAttribute<TAttribute>(this MemberInfo member) 
            where TAttribute : Attribute => member.CustomAttributes.Any(a => a.AttributeType == typeof(TAttribute));
    }
}
