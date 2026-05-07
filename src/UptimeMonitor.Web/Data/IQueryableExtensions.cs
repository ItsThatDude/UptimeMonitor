namespace UptimeMonitor.Web.Data
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, int pageSize, int currentPage)
        {
            if (pageSize == 0)
                return queryable;

            return queryable.Skip(pageSize * (currentPage - 1)).Take(pageSize);
        }
    }
}
