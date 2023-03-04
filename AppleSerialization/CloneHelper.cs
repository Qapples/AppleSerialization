using System;
using System.Collections.Generic;
using System.Linq;

namespace AppleSerialization
{
    internal static class CloneHelper
    {
        /// <summary>
        /// Creates a <see cref="List{T}"/> where each member is a clone of each instance of a
        /// <see cref="IEnumerable{T}"/> where T implements <see cref="ICloneable"/>.
        /// </summary>
        /// <param name="instances">The instances of <see cref="T"/> to clone.</param>
        /// <typeparam name="T">The type of the instances</typeparam>
        /// <returns>A <see cref="List{T}"/> where each member represents a cloned instance of a member in the
        /// provided <see cref="IEnumerable{T}"/></returns>
        internal static List<T> MemberClone<T>(this IEnumerable<T> instances) where T : ICloneable =>
            instances.Select(e => (T) e.Clone()).ToList();
    }
}