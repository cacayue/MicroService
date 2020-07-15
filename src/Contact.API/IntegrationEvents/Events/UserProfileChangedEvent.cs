namespace Contact.API.IntegrationEvents.Events
{
    public class UserProfileChangedEvent
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        public string Company { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }
    }
}