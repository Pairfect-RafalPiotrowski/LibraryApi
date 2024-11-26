namespace LibraryApiClient.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Bookstand { get; set; }
        public int Shelf { get; set; }
        public List<Author> Authors { get; set; } = new List<Author>();
    }
}
