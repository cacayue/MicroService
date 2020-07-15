using System;

namespace User.Api.Models
{
    public class UserTag
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int AppUserId { get; set; }

        public string Tag { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}