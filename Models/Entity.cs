namespace SpellsRedApi.Models
{
    public abstract class Entity
    {

        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModfieds { get; set; }
    }
}