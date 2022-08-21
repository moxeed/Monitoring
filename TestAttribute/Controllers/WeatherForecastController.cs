using Microsoft.AspNetCore.Mvc;

namespace TestAttribute.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private void Method()
    {
        using var span = new Span(nameof(Method));
        Console.WriteLine("inside");
    }

    [HttpGet]
    public IActionResult Get()
    {
        Method();
        return Ok();
    }
    
    [HttpPost]
    public IActionResult Post(Model model)
    {
        return Ok();
    }
}