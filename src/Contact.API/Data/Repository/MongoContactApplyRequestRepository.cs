using System;
using System.Collections.Generic;
using System.Threading;
using Contact.API.Models;
using System.Threading.Tasks;
using Contact.API.Enum;
using Microsoft.Extensions.Primitives;
using MongoDB.Driver;

namespace Contact.API.Data.Repository
{
    public class MongoContactApplyRequestRepository : IContactApplyRequestRepository
    {
        private readonly ContactContext _contactContext;

        public MongoContactApplyRequestRepository(ContactContext contactContext)
        {
            _contactContext = contactContext;
        }

        public async Task<bool> AddRequestAsync(ContactApplyRequest request, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactApplyRequest>.Filter.Where(c =>
                c.UserId == request.UserId && c.ApplierId == request.ApplierId);
            if (await _contactContext.ContactApplyRequests.CountDocumentsAsync(filter,cancellationToken:cancellationToken) > 0)
            {
                var update = Builders<ContactApplyRequest>.Update
                    .Set(c=>c.ApplyTime,DateTime.Now);
                var updateOneAsync = await _contactContext.ContactApplyRequests
                    .UpdateOneAsync(filter,update,cancellationToken:cancellationToken);
                return updateOneAsync.ModifiedCount == updateOneAsync.MatchedCount && updateOneAsync.MatchedCount == 1;
            }

            await _contactContext.ContactApplyRequests.InsertOneAsync(request, cancellationToken: cancellationToken);
            return true;
        }

        public async Task<bool> ApprovalAsync(int userId, int applierId, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactApplyRequest>.Filter.Where(c =>
                c.UserId == userId && c.ApplierId == applierId);
            var update = Builders<ContactApplyRequest>.Update
                .Set(c => c.HandledTime, DateTime.Now)
                .Set(c => c.Approvaled, (int)ApprovaledEnum.Approvaled);
            var updateOneAsync = await _contactContext.ContactApplyRequests
                .UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return updateOneAsync.ModifiedCount == updateOneAsync.MatchedCount && updateOneAsync.MatchedCount == 1;
        }

        public async Task<List<ContactApplyRequest>> GetApplyRequestList(int userId,CancellationToken cancellationToken)
        {
            var contactApplyRequests = await _contactContext.ContactApplyRequests
                .Find(u => u.UserId == userId)
                .ToListAsync(cancellationToken);
            return contactApplyRequests;
        }
    }
}