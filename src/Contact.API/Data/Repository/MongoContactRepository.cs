using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Dtos;
using Contact.API.Entity.Dtos;
using Contact.API.Models;
using MongoDB.Driver;

namespace Contact.API.Data.Repository
{
    public class MongoContactRepository:IContactRepository
    {
        private readonly ContactContext _contactContext;

        public MongoContactRepository(ContactContext contactContext)
        {
            _contactContext = contactContext;
        }
        public async Task<bool> UpdateContactInfoAsync(BaseUserInfo baseUser,CancellationToken cancellationToken)
        {
            var contactBook =(await _contactContext
                .ContactBooks.FindAsync(c=>c.UserId == baseUser.Id, cancellationToken: cancellationToken))
                .FirstOrDefault(cancellationToken);
            if (contactBook == null)
            {
                return true;
                //throw new Exception($"wrong userId for update contact info userId:{baseUser.UserId}");
            }

            var contactIds = contactBook.Contacts.Select(c=> c.UserId);
            var builder = Builders<ContactBook>.Filter;
            //var filterDefinition = builder.And(builder.In(c => c.UserId, contactIds), 
            //    builder.AnyEq("Contact.UserId", baseUser.UserId));  
            var filterDefinition = builder.And(builder.In(c => c.UserId, contactIds),
                builder.ElemMatch(c=>c.Contacts,contact=> contact.UserId == baseUser.Id));
            var updateDefinition = Builders<ContactBook>.Update
                .Set("Contacts.$.Name", baseUser.Name)
                .Set("Contacts.$.Company", baseUser.Company)
                .Set("Contacts.$.Title", baseUser.Title)
                .Set("Contacts.$.Avatar", baseUser.Avatar);
            var updateResult = await _contactContext.ContactBooks
                .UpdateManyAsync(filterDefinition, updateDefinition,cancellationToken:cancellationToken);
            return updateResult.MatchedCount == updateResult.ModifiedCount;
        }

        public async Task<bool> AddContactBookAsync(int userId, BaseUserInfo approvalInfo, CancellationToken cancellationToken)
        {
            if (await _contactContext.ContactBooks.CountDocumentsAsync(c => 
                    c.UserId == userId,cancellationToken:cancellationToken) == 0)
            {
                await _contactContext.ContactBooks.InsertOneAsync(new ContactBook
                {
                    UserId = userId
                },cancellationToken:cancellationToken);
            }

            var filter = Builders<ContactBook>.Filter.Eq(c => c.UserId ,userId);
            var update = Builders<ContactBook>.Update.AddToSet(c => c.Contacts, new Models.Contact
            {
                UserId = approvalInfo.Id,
                Company = approvalInfo.Company,
                Name = approvalInfo.Name,
                Title = approvalInfo.Title,
                Avatar = approvalInfo.Avatar
            });
            var contactBook =
                await _contactContext.ContactBooks.UpdateOneAsync(filter,update, cancellationToken: cancellationToken);
            return contactBook.ModifiedCount == contactBook.MatchedCount && contactBook.MatchedCount == 1;
        }

        public async Task<List<Models.Contact>> GetContactsAsync(int userId, CancellationToken cancellationToken)
        {
            var contactBook =(await _contactContext
                .ContactBooks.FindAsync(u=>u.UserId == userId,cancellationToken:cancellationToken))
                .FirstOrDefault(cancellationToken);

            return contactBook != null ? contactBook.Contacts : new List<Models.Contact>();
        }

        public async Task<bool> TagContactAsync(int userId, int contactId, List<string> tags, CancellationToken cancellationToken)
        {
            var builder = Builders<ContactBook>.Filter;
            var filter = builder.And(builder.Eq(c => c.UserId ,userId),
                builder.Eq("Contacts.UserId", contactId));
            var update = Builders<ContactBook>.Update.Set("Contacts.$.Tags", tags);
            var result =await _contactContext.ContactBooks
                .UpdateOneAsync(filter,update,cancellationToken:cancellationToken);
            return result.ModifiedCount == result.MatchedCount && result.MatchedCount == 1;
        }
    }
}