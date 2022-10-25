namespace SpellsRedApi.Api
{
    public class RequestFilter
    {
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 0;
        public string SearchColumn { get; set; } = "";
        public string SearchTerm { get; set; } = "";
    }
}