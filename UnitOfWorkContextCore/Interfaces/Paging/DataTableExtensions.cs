namespace UnitOfWorkContextCore.Interfaces.Paging
{
    public static class DataTableExtensions
    {
        public static DataTableResponse ToDataTableResponse<T>(this IPaginate<T> paginate, int draw)
        {
            return new DataTableResponse
            {
                draw = draw,
                recordsTotal = paginate.Count,
                recordsFiltered = paginate.Filtered,
                data = paginate.Items
            };
        }
    }

    public class DataTableResponse
    {
        public int draw { get; set; }

        public int recordsTotal { get; set; }

        public int recordsFiltered { get; set; }

        public object data { get; set; }
    }
}
