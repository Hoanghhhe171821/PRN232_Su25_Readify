namespace PRN232_Su25_Readify_WebAPI.Dtos.Auths
{
    public class AssignRoleRequest
    {
        public string UserName { get; set; }
        public List<string> SelectedRoles { get; set; }
    }
}
