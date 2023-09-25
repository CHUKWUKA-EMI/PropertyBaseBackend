using System;
namespace PropertyBase.DTOs.TenancyAgreement
{
    public class CreateTenancyAgreement
    {
        public Guid PropertyId { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public IFormFile File { get; set; }
        public Guid? AgencyId { get; set; }
        public string? PropertyOwnerId { get; set; }
    }
}

