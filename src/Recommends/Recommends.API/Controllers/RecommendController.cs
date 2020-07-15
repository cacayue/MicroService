using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recommends.API.Data;

namespace Recommends.API.Controllers
{
    [ApiController]
    [Route("api/recommends")]
    public class RecommendController : BaseController
    {
        private readonly RecommendDbContext _context;

        public RecommendController(RecommendDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.Recommends.AsNoTracking()
                .Where(r => r.UserId == UserIdentity.UserId)
                .ToListAsync());
        }
    }
}
