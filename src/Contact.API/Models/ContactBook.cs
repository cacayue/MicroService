using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Contact.API.Models
{
    [BsonIgnoreExtraElements]
    public class ContactBook
    {
        public ContactBook()
        {
            Contacts = new List<Contact>();
        }
        public int UserId { get; set; }
        /// <summary>
        /// 好友申请列表 
        /// </summary>
        public List<Contact> Contacts { get; set; }
    }
}