namespace EFCoreCommerceAnalytics.Entities
{
    public class ToDo
    {
        public int TodoId { get; set; }
        public string ToDoDescription { get; set; } = string.Empty;
        public bool ToDoStatus { get; set; }

        /// <summary>Değerler <see cref="ToDoPriorities"/> içindeki sabitlerden biri.</summary>
        public string? Priority { get; set; }
    }
}
