using System;
using System.Collections.Generic;

namespace UnitOfWorkContextCore.Interfaces.Paging
{
    public static class PaginateExtensions
    {
        public static IPaginate<T> ToPaginate<T>(this IEnumerable<T> source, int index, int size, int count, int from = 0)
        {
            return new Paginate<T>(source, index, size, count, from);
        }

        public static IPaginate<TResult> ToPaginate<TSource, TResult>(this IEnumerable<TSource> source,
            Func<IEnumerable<TSource>, IEnumerable<TResult>> converter, int index, int size, int count, int from = 0)
        {
            return new Paginate<TSource, TResult>(source, converter, index, size, count, from);
        }
    }
}
