namespace User.Api.Models
{
    public class UserProperty
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int AppUserId { get; set; }

        public string Key { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Text { get; set; }

        public string Value { get; set; }
    }
}