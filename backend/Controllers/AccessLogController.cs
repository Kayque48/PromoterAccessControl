using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoterAccessControl.Data;
using PromoterAccessControl.Models;

namespace PromoterAccessControl.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessLogController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AccessLogController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_db.AccessLogs.ToList());

        [HttpPost("entry")]
        public IActionResult RegisterEntry(int promoterId)
        {
            var log = new AccessLog
            {
                PromoterId = promoterId,
                EntryTime = DateTime.Now,
                ExitTime = null
            };

            _db.AccessLogs.Add(log);
            _db.SaveChanges();
            return Ok(log);
        }

        [HttpPost("exit")]
        public IActionResult RegisterExit(int promoterId)
        {
            var openLog = _db.AccessLogs
                .Where(l => l.PromoterId == promoterId && l.ExitTime == null)
                .OrderByDescending(l => l.EntryTime)
                .FirstOrDefault();

            if (openLog == null)
                return NotFound("No open entry found for this promoter.");

            openLog.ExitTime = DateTime.Now;
            _db.SaveChanges();

            var duration = openLog.ExitTime.Value - openLog.EntryTime;
            return Ok(new
            {
                openLog.Id,
                openLog.PromoterId,
                openLog.EntryTime,
                openLog.ExitTime,
                Duration = Math.Round(duration.TotalMinutes, 2) + " minutos"
            });
        }
    }
}