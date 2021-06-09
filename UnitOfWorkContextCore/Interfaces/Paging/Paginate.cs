using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitOfWorkContextCore.Interfaces.Paging
{
    public class Paginate<T> : IPaginate<T>
    {
        internal Paginate(IEnumerable<T> source, int index, int size, int count, int from)
        {
            var enumerable = source as T[] ?? source.ToArray();

            if (from > index)
                throw new ArgumentException($"indexFrom: {from} > pageIndex: {index}, must indexFrom <= pageIndex");

            if (source is IQueryable<T> querable)
            {
                Index = index;
                Size = size;
                From = from;
                Filtered = querable.Count();
                Count = count;
                Pages = (int)Math.Ceiling(Count / (double)Size);

                if (Size > 0)
                    Items = querable.Skip((Index - From) * Size).Take(Size).ToList();
                else
                    Items = querable.ToList();
            }
            else
            {
                Index = index;
                Size = size;
                From = from;
                Filtered = enumerable.Count();
                Count = count;
                Pages = (int)Math.Ceiling(Count / (double)Size);

                if (Size > 0)
                    Items = enumerable.Skip((Index - From) * Size).Take(Size).ToList();
                else
                    Items = enumerable.ToList();
            }
        }

        internal Paginate()
        {
            Items = new T[0];
        }

        public int From { get; set; }
        public int Index { get; set; }
        public int Size { get; set; }
        public int Filtered { get; }
        public int Count { get; set; }
        public int Pages { get; set; }
        public IList<T> Items { get; set; }
        public bool HasPrevious => Index - From > 0;
        public bool HasNext => Index - From + 1 < Pages;
    }

    internal class Paginate<TSource, TResult> : IPaginate<TResult>
    {
        public Paginate(IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> converter,
            int index, int size, int count, int from)
        {
            var enumerable = source as TSource[] ?? source.ToArray();

            if (from > index) throw new ArgumentException($"From: {from} > Index: {index}, must From <= Index");

            if (source is IQueryable<TSource> queryable)
            {
                Index = index;
                Size = size;
                From = from;
                Filtered = queryable.Count();
                Count = count;
                Pages = (int)Math.Ceiling(Count / (double)Size);

                if (Size > 0)
                    Items = new List<TResult>(converter(queryable.Skip((Index - From) * Size).Take(Size).ToArray()));
                else
                    Items = new List<TResult>(converter(queryable.ToArray()));
            }
            else
            {
                Index = index;
                Size = size;
                From = from;
                Filtered = enumerable.Count();
                Count = count;
                Pages = (int)Math.Ceiling(Count / (double)Size);

                if (Size > 0)
                    Items = new List<TResult>(converter(enumerable.Skip((Index - From) * Size).Take(Size).ToArray()));
                else
                    Items = new List<TResult>(converter(enumerable.ToArray()));
            }
        }


        public Paginate(IPaginate<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> converter)
        {
            Index = source.Index;
            Size = source.Size;
            From = source.From;
            Filtered = source.Filtered;
            Count = source.Count;
            Pages = source.Pages;

            Items = new List<TResult>(converter(source.Items));
        }

        public int Index { get; }
        public int Size { get; }
        public int Filtered { get; }
        public int Count { get; }
        public int Pages { get; }
        public int From { get; }
        public IList<TResult> Items { get; }
        public bool HasPrevious => Index - From > 0;
        public bool HasNext => Index - From + 1 < Pages;
    }

    public static class Paginate
    {
        public static IPaginate<T> Empty<T>()
        {
            return new Paginate<T>();
        }

        public static IPaginate<TResult> From<TResult, TSource>(IPaginate<TSource> source,
            Func<IEnumerable<TSource>, IEnumerable<TResult>> converter)
        {
            return new Paginate<TSource, TResult>(source, converter);
        }
    }
}
