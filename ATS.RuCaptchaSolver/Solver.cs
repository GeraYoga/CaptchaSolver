using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Flurl.Http;

namespace ATS.RuCaptchaSolver
{
    /// <summary>
    /// Основной класс для взаимодействия с API.
    /// </summary>
    [SuppressMessage("ReSharper", "CommentTypo")]
    public class Solver
    {
        private static string _key;
        private static bool _debugger;
        private static string _pingBackUrl;
        
        /// <summary>
        /// Решение обычной капчи.
        /// </summary>
        public SimpleCaptchaSolver SimpleSolver = new SimpleCaptchaSolver();
        /// <summary>
        /// Решение текстовой капчи.
        /// </summary>
        public TextCaptchaSolver TextSolver = new TextCaptchaSolver();
        /// <summary>
        /// Решение RE капчи.
        /// </summary>
        public ReCaptchaSolver ReSolver = new ReCaptchaSolver();
        
        /// <param name="key">Ключ разработчика</param>
        /// <param name="pingBackUrl">URL для отправки ответов</param>
        /// <param name="debugger">Включить логирование</param>
        public Solver(string key, string pingBackUrl = null, bool debugger = false)
        {
            _key = key;
            _debugger = debugger;
            _pingBackUrl = pingBackUrl;

            if (_pingBackUrl == null) return;
            
            var result = CallBackHelper.PingBackAction(_key, _pingBackUrl, PingBack.Add).Result;
            
            if (_debugger)
            {
                Debugger.Log(0, null, $"Type: [RES]\r\nURL: [{_pingBackUrl}]\r\nResponse: [{result}]\r\n");
            } 
                
            if (!HandleError.ProcessResponse(result, out var _))
            {
                throw new Exception($"Сервер вернул ошибку: {result}");
            }
        }
        

        /// <summary>
        /// Решение обычной капчи.
        /// </summary>
        public class SimpleCaptchaSolver
        {
            /// <summary>
            ///  Автоматически загружает изображение на сервер и получает ответ.
            /// </summary>
            /// <param name="imageUrl">URL с картинкой</param>
            public async Task<ResponseData> SolveCaptchaAuto(string imageUrl)
            {
                var id = await SendCaptchaImage(imageUrl);
                return await SolveCaptcha(id.AnswerText);
            }

            /// <summary>
            ///  Автоматически загружает изображение на сервер и отпрвляет ответ на pingBackUrl.
            /// </summary>
            /// <param name="imageUrl">URL с картинкой</param>
            public async Task<ResponseData> SolveCaptchaCallBack(string imageUrl)
            {
                var stream = await imageUrl.GetStreamAsync();

                var response = await "https://rucaptcha.com/in.php".PostMultipartAsync(mp =>
                {
                    mp.AddStringParts(new
                    {
                        key = _key,
                        method = "post",
                        pingback = _pingBackUrl,
                        json = 1
                    });
                    mp.AddFile("file", stream, "captcha_img.jpg");
                });
            
                var responseStr = await response.GetStringAsync();
                
                if (_debugger)
                {
                    Debugger.Log(0, null, $"Type: [IN]\r\nURL: [{imageUrl}]\r\nResponse: [{responseStr}]\r\n");
                }
                
                HandleError.ProcessResponse(responseStr, out var value);
                
                return value;
            }

            /// <summary>
            /// Отправляет картинку на сервер.
            /// </summary>
            /// <param name="imageUrl">URL с картинкой</param>
            public async Task<ResponseData> SendCaptchaImage(string imageUrl)
            {
                var stream = await imageUrl.GetStreamAsync();

                var response = await "https://rucaptcha.com/in.php".PostMultipartAsync(mp =>
                {
                    mp.AddStringParts(new
                    {
                        key = _key,
                        method = "post",
                        json = 1
                    });
                    mp.AddFile("file", stream, "captcha_img.jpg");
                });
            
                var responseStr = await response.GetStringAsync();
                
                if (_debugger)
                {
                    Debugger.Log(0, null, $"Type: [IN]\r\nURL: [{imageUrl}]\r\nResponse: [{responseStr}]\r\n");
                }
                
                HandleError.ProcessResponse(responseStr, out var value);
                
                return value;
            
            }

            /// <summary>
            ///  Решает капчу по ID.
            /// </summary>
            /// <param name="imageId">URL с картинкой</param>
            /// <param name="retrySec">Повторный запрос через N-секунд</param>
            /// <param name="retryCount">Количество попыток для решения капчи</param>
            /// <returns></returns>
            public async Task<ResponseData> SolveCaptcha(string imageId, byte retrySec = 5, byte retryCount = 5)
            {
                while (true)
                {
                    retryCount--;
                    
                    var response = await "https://rucaptcha.com/res.php".PostUrlEncodedAsync(new
                    {
                        key = _key,
                        action = "get",
                        id = imageId,
                        json = 1

                    });

                    var result = await response.GetStringAsync();
                    
                    if (_debugger)
                    {
                        Debugger.Log(0, null, $"Type: [RES]\r\nID: [{imageId}]\r\nResponse: [{result}]\r\n");
                    }
                    
                    if (HandleError.ProcessResponse(result, out var value)) return value;
                    
                    if (retryCount == 0 || string.IsNullOrEmpty(imageId))
                    {
                        return value;
                    }

                    await Task.Delay(retrySec * 1000);
                }
            }
        }

        /// <summary>
        /// Решение текстовой капчи.
        /// </summary>
        public class TextCaptchaSolver
        {
            /// <summary>
            /// Автоматически загружает текст капчи сервер и получает ответ.
            /// </summary>
            /// <param name="text">Текстовая инструкция</param>
            public async Task<ResponseData> SolveCaptchaAuto(string text)
            {
                var id = await SendCaptchaText(text);
                return await SolveCaptcha(id.AnswerText);
            }
            
            /// <summary>
            ///  Загружает текст капчи сервер и отпрвляет ответ на pingBackUrl.
            /// </summary>
            /// <param name="text">Текстовая инструкция</param>
            public async Task<ResponseData> SolveCaptchaCallBack(string text)
            {
                var response = await "https://rucaptcha.com/in.php".PostUrlEncodedAsync(new
                {
                    key = _key,
                    textcaptcha = text,
                    pingback = _pingBackUrl,
                    json = 1
                });
                
                var result = await response.GetStringAsync();
                
                if (_debugger)
                {
                    Debugger.Log(0, null, $"Type: [IN]\r\nText: [{text}]\r\nResponse: [{result}]\r\n");
                }    
                
                HandleError.ProcessResponse(result, out var value);

                return value;
            }
            
            /// <summary>
            /// Загружает текст капчи сервер.
            /// </summary>
            /// <param name="text">Текстовая инструкция</param>
            /// <returns></returns>
            public async Task<ResponseData> SendCaptchaText(string text)
            {
                var response = await "https://rucaptcha.com/in.php".PostUrlEncodedAsync(new
                {
                    key = _key,
                    textcaptcha = text,
                    json = 1
                });
                
                var result = await response.GetStringAsync();
                
                if (_debugger)
                {
                    Debugger.Log(0, null, $"Type: [IN]\r\nText: [{text}]\r\nResponse: [{result}]\r\n");
                }    
                
                HandleError.ProcessResponse(result, out var value);

                return value;
            }

            /// <summary>
            /// Решает капчу по ID.
            /// </summary>
            /// <param name="textId">ID текста капчи</param>
            /// <param name="retrySec">Повторный запрос через N-секунд</param>
            /// <param name="retryCount">Количество попыток для решения капчи</param>
            /// <returns></returns>
            public async Task<ResponseData> SolveCaptcha(string textId, byte retrySec = 10, byte retryCount = 5)
            {
                while (true)
                {
                    retryCount--;
                    
                    var response = await "https://rucaptcha.com/res.php".PostUrlEncodedAsync(new
                    {
                        key = _key,
                        action = "get",
                        id = textId,
                        json = 1
                    });

                    var result = await response.GetStringAsync();
                    
                    if (_debugger)
                    {
                        Debugger.Log(0, null, $"Type: [RES]\r\nID: [{textId}]\r\nResponse: [{result}]\r\n");
                    } 
                    
                    if(HandleError.ProcessResponse(result, out var value)) return value;
                    
                    if (retryCount == 0 || string.IsNullOrEmpty(textId))
                    {
                        return value;
                    }
                    
                    await Task.Delay(retrySec * 1000);
                }
            }
        }
        
        /// <summary>
        /// Решение RE капчи.
        /// </summary>
        public class ReCaptchaSolver
        {
            /// <summary>
            /// Автоматически загружает страницу капчи сервер и получает ответ.
            /// </summary>
            /// <param name="googleKey">Значение параметра k или data-sitekey, которое вы нашли в коде страницы</param>
            /// <param name="url">Полный URL страницы, на которой вы решаете ReCaptcha V2</param>
            /// <returns></returns>
            public async Task<ResponseData> SolveCaptchaAuto(string googleKey, string url)
            {
                var id = await SendCaptchaData(googleKey, url);
                return await SolveCaptcha(id.AnswerText);
            }
            
            /// <summary>
            ///  Загружает страницу капчи сервер и отпрвляет ответ на pingBackUrl.
            /// </summary>
            /// <param name="googleKey">Значение параметра k или data-sitekey, которое вы нашли в коде страницы</param>
            /// <param name="url">Полный URL страницы, на которой вы решаете ReCaptcha V2</param>
            /// <param name="invis">Определяет тип капчи. 1 - невидимая, 0 - видимая</param>
            /// <returns></returns>
            public async Task<ResponseData> SolveCaptchaCallBack(string googleKey, string url, byte invis = 0)
            {
                var response = await "https://rucaptcha.com/in.php".PostUrlEncodedAsync(new
                {
                    key = _key,
                    method = "userrecaptcha",
                    googlekey = googleKey,
                    pageurl = url,
                    invisible = invis,
                    pingback = _pingBackUrl,
                    json = 1
                });
                
                var result = await response.GetStringAsync();
                
                if (_debugger)
                {
                    Debugger.Log(0, null, $"Type: [IN]\r\nURL: [{url}]\r\nGoogleKey: [{googleKey}]\r\nResponse: [{result}]\r\n");
                }    
                
                HandleError.ProcessResponse(result, out var value);

                return value;
            }
            
            /// <summary>
            /// Загружает страницу капчи на сервер.
            /// </summary>
            /// <param name="googleKey">Значение параметра k или data-sitekey, которое вы нашли в коде страницы</param>
            /// <param name="url">Полный URL страницы, на которой вы решаете ReCaptcha V2</param>
            /// <param name="invis">Определяет тип капчи. 1 - невидимая, 0 - видимая</param>
            /// <returns></returns>
            public async Task<ResponseData> SendCaptchaData(string googleKey, string url, byte invis = 0)
            {
                var response = await "https://rucaptcha.com/in.php".PostUrlEncodedAsync(new
                {
                    key = _key,
                    method = "userrecaptcha",
                    googlekey = googleKey,
                    pageurl = url,
                    invisible = invis,
                    json = 1
                    
                });
                
                var result = await response.GetStringAsync();
                
                if (_debugger)
                {
                    Debugger.Log(0, null, $"Type: [IN]\r\nURL: [{url}]\r\nGoogleKey: [{googleKey}]\r\nResponse: [{result}]\r\n");
                }    
                
                HandleError.ProcessResponse(result, out var value);

                return value;
            }
            
            /// <summary>
            /// Решает капчу по ID.
            /// </summary>
            /// <param name="reId">ID загруженной капчи</param>
            /// <param name="retrySec">Повторный запрос через N-секунд</param>
            /// <param name="retryCount">Количество попыток для решения капчи</param>
            /// <returns></returns>
            public async Task<ResponseData> SolveCaptcha(string reId, byte retrySec = 15, byte retryCount = 5)
            {
                while (true)
                {
                    retryCount--;

                    var response = await "https://rucaptcha.com/res.php".PostUrlEncodedAsync(new
                    {
                        key = _key,
                        action = "get",
                        id = reId,
                        json = 1
                    });

                    var result = await response.GetStringAsync();
                    
                    if (_debugger)
                    {
                        Debugger.Log(0, null, $"Type: [RES]\r\nID: [{reId}]\r\nResponse: [{result}]\r\n");
                    } 
                    
                    if (HandleError.ProcessResponse(result, out var value)) return value;
                    
                    if (retryCount == 0 || string.IsNullOrEmpty(reId))
                    {
                        return value;
                    }
                    
                    await Task.Delay(retrySec * 1000);
                }
            }
        }
    }
}