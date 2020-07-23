namespace ATS.RuCaptchaSolver
{
    /// <summary>
    /// 
    /// </summary>
    public struct ResponseData
    {
        /// <summary>
        /// Статус ответа. 1 - OK, 0 - Bad
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Текст решенной капчи
        /// </summary>
        public string AnswerText { get; set;}
        /// <summary>
        /// Код ошибки, доступен если статус ответа Bad
        /// </summary>
        public string ErrorDescription { get; set;}
    }
}