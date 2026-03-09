using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using net8_api_docker_sqlserver_tls_fix.Data;
using net8_api_docker_sqlserver_tls_fix.Models;

namespace net8_api_docker_sqlserver_tls_fix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(AppDbContext dbContext, ILogger<DatabaseController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("health")]
    public async Task<IActionResult> CheckDatabaseHealth()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                _logger.LogWarning("Database connection failed");
                return StatusCode(503, new { status = "unhealthy", message = "Cannot connect to database" });
            }

            _logger.LogInformation("Database connection successful");
            return Ok(new { status = "healthy", message = "Database connection is working" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return StatusCode(503, new { status = "unhealthy", message = ex.Message });
        }
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedDatabase()
    {
        try
        {
            // Ensure database is created
            await _dbContext.Database.EnsureCreatedAsync();

            var testEntity = new TestEntity
            {
                Name = $"Test Entry - {DateTime.UtcNow:O}",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.TestEntities.Add(testEntity);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Database seeded successfully");
            return Ok(new { status = "success", message = "Database seeded with test data", entityId = testEntity.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed database");
            return StatusCode(500, new { status = "error", message = ex.Message });
        }
    }

    [HttpGet("test-entities")]
    public async Task<IActionResult> GetTestEntities()
    {
        try
        {
            var entities = await _dbContext.TestEntities.ToListAsync();
            _logger.LogInformation($"Retrieved {entities.Count} test entities");
            return Ok(new { status = "success", count = entities.Count, entities });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve test entities");
            return StatusCode(500, new { status = "error", message = ex.Message });
        }
    }
}
