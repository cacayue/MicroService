using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace User.Api.Controllers
{
    [Route("HealthCheck")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {

        [HttpGet]
        [HttpHead]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}