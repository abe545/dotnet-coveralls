using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dotnet.Coveralls
{
    public static class EnumerableExtensions
    {
        public static async Task<IEnumerable<R>> Select<T, R>(this IEnumerable<T> collection, Func<T, Task<R>> selector) =>
            await Task.WhenAll(Enumerable.Select(collection, selector));
        public static async Task<IEnumerable<R>> SelectMany<T, R>(this IEnumerable<T> collection, Func<T, Task<IEnumerable<R>>> selector) =>
            (await Task.WhenAll(Enumerable.Select(collection, selector))).SelectMany(s => s);
    }
}
