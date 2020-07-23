# CaptchaSolver

.Net библиотека для взаимодействия с [RuCaptcha API](https://rucaptcha.com/api-rucaptcha)

# Примеры

## Обычные капчи
* Обычная капча — это изображение, на котором размещён искажённый текст, который может быть прочитан человеком. Чтобы решить капчу, нужно ввести текст с изображения.
```C#
public static void Main(string[] args)
{
  Solver solver = new Solver("key", true); 
  var text = solver.SimpleSolver.SolveCaptchaAuto("http://url.com/image.jpg");
  Console.WriteLine(text.Result);
}
```

## Текстовые капчи
* Текстовая капча — это капча, которая не содержит изображений и представлена в виде простого текста. Обычно для решения нужно ответить на какой-либо вопрос.
```C#
public static void Main(string[] args)
{
  Solver solver = new Solver("key", true);
  var text = solver.TextSolver.SolveCaptchaAuto("Если завтра суббота, то какой сегодня день?");
  Console.WriteLine(text.Result);
}
```

## ReCaptcha V2
* ReCaptcha V2, также известная как "Я не робот" reCaptcha, очень популяна и выглядит вот так:
<p align="center">
	<img src="https://rucaptcha.com/api_desc_files/img/recaptchav2.gif"/>
</p>

```C#
public static void Main(string[] args)
{
  Solver solver = new Solver("key", true);
  var text = solver.ReSolver.SolveCaptchaAuto("data-sitekey", "https://urlwithcaptcha.com" );
  Console.WriteLine(text.Result);
}
```



<i>Больше информации - [WIKI](https://github.com/GeraYoga/CaptchaSolver/wiki)<i/>
