namespace PRN232_Su25_Readify_WebAPI.Exceptions
{
    public class ValidationEx : Exception
    {
        public IDictionary<string, string[]> Errors { get; }
        public ValidationEx() : base("Validation failed")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationEx(IDictionary<string, string[]> errors) : base("Validation failed")
        {
            Errors = errors;
        }
    }
}
