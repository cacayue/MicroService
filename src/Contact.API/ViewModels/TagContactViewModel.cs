using System.Collections.Generic;

namespace Contact.API.ViewModels
{
    public class TagContactViewModel
    {
        public int ContactId { get; set; }
        public List<string> Tags { get; set; }

    }
}