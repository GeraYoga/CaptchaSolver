using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ATS.RuCaptchaSolver
{
    public static class HandleError
    {
        private static IEnumerable<string> GetValue(this string str) => str.Split('|');

        public static bool IsResponseOk(string response, out string value)
        {
            var responseArr = response.GetValue();
            
            if (responseArr.First() != "OK")
            {
                value = string.Empty;
                return false;
                
            }
            
            value = response.GetValue().Last();
            return true;
        }
    }
}