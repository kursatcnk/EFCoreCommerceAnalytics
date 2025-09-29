namespace EFCoreCommerceAnalytics.Entities
{
    public class ToDo
    {
        public int TodoId { get; set; }
        public string ToDoDescription { get; set; }
        public bool ToDoStatus { get; set; }
        public string? Priority { get; set; }

    }
}
