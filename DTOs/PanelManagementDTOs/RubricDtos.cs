// DTOs/PanelManagementDTOs/RubricDTOs.cs
namespace backend.DTOs.PanelManagementDTOs
{
    public class EvaluationRubricDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<RubricCategoryDto> Categories { get; set; } = new List<RubricCategoryDto>();
    }

    public class RubricCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Weight { get; set; }
        public int MaxScore { get; set; }
    }

    public class CreateRubricDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<CreateRubricCategoryDto> Categories { get; set; } = new List<CreateRubricCategoryDto>();
    }

    public class CreateRubricCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Weight { get; set; }
        public int MaxScore { get; set; } = 10;
    }

    public class UpdateRubricDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<UpdateRubricCategoryDto> Categories { get; set; } = new List<UpdateRubricCategoryDto>();
    }

    public class UpdateRubricCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Weight { get; set; }
        public int MaxScore { get; set; }
    }
}
