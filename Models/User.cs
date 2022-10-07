namespace SpellsRedApi.Models
{
    public partial class User
    {
        public long Id { get; set; }
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
        public string KeycloakId { get; set; } = "";
        public DefaultView? DefaultView { get; set; }
        public long[]? RepoIds { get; set; } 
        public long[]? SpellBookIds { get; set; } 
        public ViewSettinigs? ViewSettinigs { get; set; }
    }

    public partial class DefaultView
    {
        public string Type { get; set; } = "Main";
        public long Id { get; set; }
    }

    public partial class ViewSettinigs
    {
        public string SortBy { get; set; } = "Level";
        public string SpellLayout { get; set; } = "Default";
    }
}
