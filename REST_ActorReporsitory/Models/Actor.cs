namespace REST_ActorReporsitory.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int birthYear { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Birth Year: {birthYear}";
        }
    }
}
