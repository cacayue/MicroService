using System.Text.RegularExpressions;

namespace User.Api.Utils
{
    public static class CheckUtils
    {
        public static bool CheckPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }
            const string rule = "^((13[0-9])|(14[5|7])|(15([0-3]|[5-9]))|(17[013678])|(18[0,5-9]))\\d{8}$";
            return Regex.IsMatch(phone, rule);
        }
    }
}