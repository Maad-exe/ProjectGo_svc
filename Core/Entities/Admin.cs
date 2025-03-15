namespace backend.Core.Entities
{
    public class Admin:User
    {
        public bool IsSuperAdmin { get; set; } = false;
    
}
}
