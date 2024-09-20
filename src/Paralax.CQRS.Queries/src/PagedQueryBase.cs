namespace Paralax.CQRS.Queries
{
    public abstract class PagedQueryBase : IPagedQuery
    {
        public int Page { get; set; } = 1;
        public int Results { get; set; } = 10;
        public string OrderBy { get; set; } = "Created"; 
        public string SortOrder { get; set; } = "ASC"; 

        public void Validate()
        {
            if (Page <= 0) Page = 1;
            if (Results <= 0) Results = 10;
            if (string.IsNullOrEmpty(OrderBy)) OrderBy = "Created";
            if (string.IsNullOrEmpty(SortOrder)) SortOrder = "ASC";
        }
    }
}
