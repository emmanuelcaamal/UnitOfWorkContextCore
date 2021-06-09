using System.Linq;
using System.Linq.Expressions;

namespace UnitOfWorkContextCore.Utilities
{
    public static class OrderingHelper
    {
        public static IOrderedQueryable<T> OrderHelper<T>(this IQueryable<T> source, string propertyName, bool descending, bool anotherLevel = false)
        {
            ParameterExpression param = Expression.Parameter(typeof(T), string.Empty);

            Expression body = param;
            foreach (var member in propertyName.Split('.'))
            {
                body = Expression.PropertyOrField(body, member);
            }

            LambdaExpression sort = Expression.Lambda(body, param);
            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                (!anotherLevel ? "OrderBy" : "ThenBy") + (descending ? "Descending" : string.Empty),
                new[] { typeof(T), body.Type },
                source.Expression,
                Expression.Quote(sort));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
        }
    }
}
