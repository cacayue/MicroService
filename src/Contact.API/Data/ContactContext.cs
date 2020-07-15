using System.Collections.Generic;
using System.Threading.Tasks;
using Contact.API.Models;
using Microsoft.Extensions.Localization.Internal;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Contact.API.Data
{
    public class ContactContext
    {
        private readonly IMongoDatabase _database;

        public ContactContext(IOptionsSnapshot<Models.MongoDatabaseSettings> settings)
        {
            var appSetting = settings.Value;
            var client = new MongoClient(appSetting.ConnectionString);
            _database = client.GetDatabase(appSetting.DatabaseName); 
        }

        private async Task CheckOrCreateCollection(string collectionName)
        {
            var collectionNames =await _database.ListCollectionNames().ToListAsync();
            
            if (!collectionNames.Contains(collectionName))
            {
               await _database.CreateCollectionAsync(collectionName);
            }
        }

        /// <summary>
        /// 通讯录
        /// </summary>
        public IMongoCollection<ContactBook> ContactBooks
        {
            get
            {
                const string name = "ContactBook";
                Task.Run(async () =>  await CheckOrCreateCollection(name));
                return _database.GetCollection<ContactBook>(name);
            }
        }

        /// <summary>
        /// 好友申请记录
        /// </summary>
        public IMongoCollection<ContactApplyRequest> ContactApplyRequests
        {
            get
            {
                const string name = "ContactApplyRequest";
                Task.Run(async () => await CheckOrCreateCollection(name));
                return _database.GetCollection<ContactApplyRequest>(name);
            }
        }
    }
}