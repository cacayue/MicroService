using System;

namespace User.Api.Models
{
    public class BPFile
    {
        public int Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 上传的源地址
        /// </summary>
        public string OriginFilePath { get; set; }

        /// <summary>
        /// 格式转化后的文件地址
        /// </summary>
        public string FormatFilePath { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}