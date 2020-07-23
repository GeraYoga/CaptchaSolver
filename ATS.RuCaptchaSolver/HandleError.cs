using Newtonsoft.Json.Linq;

namespace ATS.RuCaptchaSolver
{
    /// <summary>
    /// Вспомогательный класс для обработки ответа с сервера.
    /// </summary>
    public static class HandleError
    {
        /// <summary>
        /// Обрабатывает полученный ответ с сервера.
        /// </summary>
        /// <param name="value">Строка JSON</param>
        /// <param name="data">Сформированный ответ в виде структуры.</param>
        /// <returns></returns>
        public static bool ProcessResponse(string value, out ResponseData data)
        {
            JObject arrObj = JObject.Parse(value);
            
            data = new ResponseData
            {
                Status = (string) arrObj["status"], 
                AnswerText = (string) arrObj["request"],
                ErrorDescription = (string) arrObj["error_text"]
            };

            return data.Status == "1";
        }
    }
}