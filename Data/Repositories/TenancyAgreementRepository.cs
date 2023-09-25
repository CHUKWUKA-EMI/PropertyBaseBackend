using System;
using PropertyBase.Contracts;
using PropertyBase.Entities;

namespace PropertyBase.Data.Repositories
{
    public class TenancyAgreementRepository: BaseRepository<TenancyAgreement>, ITenancyAgreementRepository
    {
        public TenancyAgreementRepository(PropertyBaseDbContext dbContext):base(dbContext)
        {
        }
    }
}

