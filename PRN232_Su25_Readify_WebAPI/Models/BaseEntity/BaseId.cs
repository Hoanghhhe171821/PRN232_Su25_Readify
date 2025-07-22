namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class BaseId
    {
        public int Id { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
