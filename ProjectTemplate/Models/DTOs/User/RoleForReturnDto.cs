using ProjectTemplate.Models.Domain;

namespace ProjectTemplate.Models.DTOs
{
    public class RoleForReturnDto : IIdentifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
    }
}