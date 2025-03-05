using System.Text.Json;

namespace backend.DTOs
{
    public class UserDetailsDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public object? AdditionalInfo { get; set; }

        // Method to serialize additional info as JSON string
        public string? GetAdditionalInfoJson()
        {
            return AdditionalInfo != null
                ? JsonSerializer.Serialize(AdditionalInfo)
                : null;
        }
    }

    public class UserUpdateDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Dictionary<string, object>? AdditionalInfo { get; set; }
    }
}