﻿using System.Threading.Tasks;
using Flurl.Http;

namespace ATS.RuCaptchaSolver
{
    /// <summary>
    /// Вспомогательный класс для управления Callback.
    /// </summary>
    public static class CallBackHelper
    {
        /// <summary>
        ///  Выполняет заданное действие с CallBack.
        /// </summary>
        /// <param name="captchaKey">Ключ разработчика</param>
        /// <param name="url">URL адрес вашего сайта</param>
        /// <param name="type">Тип действия</param>
        /// <returns></returns>
        public static async Task<string> PingBackAction(string captchaKey, string url, PingBack type)
        {
            return await "https://rucaptcha.com/res.php".PostUrlEncodedAsync(new
            {
                key = captchaKey,
                action = type == PingBack.Add ? "add_pingback" : type == PingBack.Del ? "del_pingback" : "get_pingback",
                addr = url
            }).ReceiveString();
        }
        
        /// <summary>
        /// Отправляет отчет по решению капчи.
        /// </summary>
        /// <param name="captchaKey">Ключ разработчика</param>
        /// <param name="captchaId">ID капчи</param>
        /// <param name="reportType">Тип репорта</param>
        /// <returns></returns>
        public static async Task<string> SendReport(string captchaKey, string captchaId,  ReportType reportType)
        {
            return await "https://rucaptcha.com/res.php".PostUrlEncodedAsync(new
            {
                key = captchaKey,
                id = captchaId,
                action = reportType == ReportType.Bad ? "reportbad" : "reportgood",
            }).ReceiveString();
        }
    }
}