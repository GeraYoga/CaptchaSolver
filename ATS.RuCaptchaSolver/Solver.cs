using System.Diagnostics;
using System.Threading.Tasks;
using Flurl.Http;

namespace ATS.RuCaptchaSolver
{
    public class Solver
    {
        private static string _key;
        private static bool _debugger;
        
        public SimpleCaptchaSolver SimpleSolver = new SimpleCaptchaSolver();
        public TextCaptchaSolver TextSolver = new TextCaptchaSolver();
        public ReCaptchaSolver ReSolver = new ReCaptchaSolver();
        
        public Solver(string key, bool debugger = false)
        {
            _key = key;
            _debugger = debugger;
        }
        

        public class SimpleCaptchaSolver
        {
            public async Task<string> SolveCaptchaAuto(string imageUrl)
            {
                var id = await SendCaptchaImage(imageUrl);
                return await SolveCaptcha(id);
            }

            public async Task<string> SendCaptchaImage(string imageUrl)
            {
                var stream = await imageUrl.GetStreamAsync();

                var response = await "https://rucaptcha.com/in.php".PostMultipartAsync(mp =>
                {
                    mp.AddStringParts(new
                    {
                        key = _key,
                        method = "post",
                    });
                    mp.AddFile("file", stream, "captcha_img.jpg");
                });
            
                var responseStr = await response.GetStringAsync();
                
                if (_debugger)
                {
                    Debugger.Log(0, null, $"Type: [IN]\r\nURL: [{imageUrl}]\r\nResponse: [{responseStr}]\r\n");
                }
                
                HandleError.IsResponseOk(responseStr, out var value);
                
                return value;
            
            }

            public async Task<string> SolveCaptcha(string imageId, byte retrySec = 5, byte retryCount = 5)
            {
                while (true)
                {
                    if (retryCount == 0 || string.IsNullOrEmpty(imageId))
                    {
                        return string.Empty;
                    }
                
                    var response = await "https://rucaptcha.com/res.php".PostUrlEncodedAsync(new
                    {
                        key = _key,
                        action = "get",
                        id = imageId

                    });

                    var result = await response.GetStringAsync();
                    
                    if (_debugger)
                    {
                        Debugger.Log(0, null, $"Type: [RES]\r\nID: [{imageId}]\r\nResponse: [{result}]\r\n");
                    }
                    
                    if (HandleError.IsResponseOk(result, out var value)) return value;

                    await Task.Delay(retrySec * 1000);
                    retryCount--;
                }
            }
        }

        public class TextCaptchaSolver
        {
            public async Task<string> SolveCaptchaAuto(string text)
            {
                var id = await SendCaptchaText(text);
                return await SolveCaptcha(id);
            }
            
            public async Task<string> SendCaptchaText(string text)
            {
                var response = await "https://rucaptcha.com/in.php".PostUrlEncodedAsync(new
                {
                    key = _key,
                    textcaptcha = text
                });
                
                var result = await response.GetStringAsync();
                
                if (_debugger)
                {
                    Debugger.Log(0, null, $"Type: [IN]\r\nText: [{text}]\r\nResponse: [{result}]\r\n");
                }    
                
                HandleError.IsResponseOk(result, out var value);

                return value;
            }

            public async Task<string> SolveCaptcha(string textId, byte retrySec = 10, byte retryCount = 5)
            {
                while (true)
                {
                    if (retryCount == 0 || string.IsNullOrEmpty(textId))
                    {
                        return string.Empty;
                    }

                    var response = await "https://rucaptcha.com/res.php".PostUrlEncodedAsync(new
                    {
                        key = _key,
                        action = "get",
                        id = textId
                    });

                    var result = await response.GetStringAsync();
                    
                    if (_debugger)
                    {
                        Debugger.Log(0, null, $"Type: [RES]\r\nID: [{textId}]\r\nResponse: [{result}]\r\n");
                    } 
                    
                    if(HandleError.IsResponseOk(result, out var value)) return value;
                    
                    await Task.Delay(retrySec * 1000);
                    retryCount--;
                }
            }
        }
        
        public class ReCaptchaSolver
        {
            public async Task<string> SolveCaptchaAuto(string googleKey, string url)
            {
                var id = await SendCaptchaData(googleKey, url);
                return await SolveCaptcha(id);
            }
            
            public async Task<string> SendCaptchaData(string googleKey, string url)
            {
                var response = await "https://rucaptcha.com/in.php".PostUrlEncodedAsync(new
                {
                    key = _key,
                    method = "userrecaptcha",
                    googlekey = googleKey,
                    pageurl = url
                    
                });
                
                var result = await response.GetStringAsync();
                
                if (_debugger)
                {
                    Debugger.Log(0, null, $"Type: [IN]\r\nURL: [{url}]\r\nGoogleKey: [{googleKey}]\r\nResponse: [{result}]\r\n");
                }    
                
                HandleError.IsResponseOk(result, out var value);

                return value;
            }
            
            public async Task<string> SolveCaptcha(string reId, byte retrySec = 15, byte retryCount = 5)
            {
                while (true)
                {
                    if (retryCount == 0 || string.IsNullOrEmpty(reId))
                    {
                        return string.Empty;
                    }

                    var response = await "https://rucaptcha.com/res.php".PostUrlEncodedAsync(new
                    {
                        key = _key,
                        action = "get",
                        id = reId
                    });

                    var result = await response.GetStringAsync();
                    
                    if (_debugger)
                    {
                        Debugger.Log(0, null, $"Type: [RES]\r\nID: [{reId}]\r\nResponse: [{result}]\r\n");
                    } 
                    
                    if(HandleError.IsResponseOk(result, out var value)) return value;
                    
                    await Task.Delay(retrySec * 1000);
                    retryCount--;
                }
            }
        }
    }
}