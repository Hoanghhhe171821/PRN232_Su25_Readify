namespace PRN232_Su25_Readify_Web.Models.Auth
{
    public class JwtPayLoad
    {
        public string Sub { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public List<string> Role { get; set; }
    }
}
