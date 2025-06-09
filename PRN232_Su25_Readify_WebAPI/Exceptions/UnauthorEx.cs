namespace PRN232_Su25_Readify_WebAPI.Exceptions
{
    public class UnauthorEx : Exception
    {
        public UnauthorEx(string message = "Unauthorized access") : base(message) { }

    }
}
