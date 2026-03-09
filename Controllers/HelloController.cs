using Microsoft.AspNetCore.Mvc;

namespace net8_api_docker_sqlserver_tls_fix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Hello World!" });
    }
}
