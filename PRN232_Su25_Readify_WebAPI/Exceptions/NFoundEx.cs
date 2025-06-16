namespace PRN232_Su25_Readify_WebAPI.Exceptions
{
    public class NFoundEx : Exception
    {
        public NFoundEx(string message, object key)
        : base($"{message} with key ({key}) was not found.") { }

        public NFoundEx(string message) : base(message) { }
    }
}
