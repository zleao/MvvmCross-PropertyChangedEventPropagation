using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PropertyChangedEventPropagation.Core.Extensions
{
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Gets the custom attribute.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes.</param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(this MemberInfo memberInfo, bool inherit = false)
            where T : Attribute
        {
            var attributes = GetCustomAttributes<T>(memberInfo, inherit);
            if (attributes == null)
                return null;
            return attributes.FirstOrDefault();
        }

        /// <summary>
        /// Gets the custom attributes.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo memberInfo, bool inherit = false)
            where T : Attribute
        {
            if (memberInfo == null)
                throw new ArgumentNullException("memberInfo");

            var customAttributes = memberInfo.GetCustomAttributes(typeof(T), inherit);
            if (customAttributes != null)
                return customAttributes.OfType<T>();
            return null;
        }
    }
}
