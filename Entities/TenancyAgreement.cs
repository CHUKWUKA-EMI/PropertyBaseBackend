using System;
namespace PropertyBase.Entities
{
    public class TenancyAgreement: BaseEntity
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string documentUrl { get; set; } = string.Empty;
        public string FileId { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public bool Signed { get; set; } = false;
        public string? SignedByEmail { get; set; }
        public DateTime? SignedDate { get; set; }
        public Guid? AgencyId { get; set; }
        public string? PropertyOwnerId { get; set; }
    }
}

