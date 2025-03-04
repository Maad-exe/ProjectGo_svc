namespace backend.Core.Entities
{
    public class Student : User
    {
        public string EnrollmentNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        //public double CGPA { get; set; } = 
    }
}
