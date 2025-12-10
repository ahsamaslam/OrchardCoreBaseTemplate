// src/Modules/Orchard.TenantManagement/Controllers/TenantsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orchard.ModuleBase;
using Orchard.TenantManagement.Models;
using Orchard.TenantManagement.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.TenantManagement.Controllers
{
    [ApiController]
    [Route("api/tenants")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantRepository _repo;
        private readonly IMigrationRunnerService _migrationRunner;
        //private readonly IRecipeExecutor? _recipeExecutor; // optional: host may or may not provide
        private readonly ILogger<TenantsController> _logger;

        public TenantsController(ITenantRepository repo,
                                 IMigrationRunnerService migrationRunner,
                                 IServiceProvider sp,
                                 ILogger<TenantsController> logger)
        {
            _repo = repo;
            _migrationRunner = migrationRunner;
            _logger = logger;

            // recipe executor is optional - use service locator to allow module to run without Host recipe system
           // _recipeExecutor = sp.GetService(typeof(IRecipeExecutor)) as IRecipeExecutor;
        }

        public record CreateTenantDto(string TenantId, string TenantName, string ConnectionString, string Hosts, string? RecipeName);

        [HttpPost]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TenantId) || string.IsNullOrWhiteSpace(dto.ConnectionString))
                return BadRequest("TenantId and ConnectionString are required.");

            var tenant = new TenantInfo
            {
                TenantId = dto.TenantId,
                TenantName = dto.TenantName ?? dto.TenantId,
                ConnectionString = dto.ConnectionString,
                Hosts = string.IsNullOrWhiteSpace(dto.Hosts) ? null : dto.Hosts,
                SettingsJson = null,
                IsActive = true
            };

            await _repo.AddAsync(tenant);
            _logger.LogInformation("Tenant {TenantId} persisted.", tenant.TenantId);

            // run migrations for new tenant
            _logger.LogInformation("Running tenant migrations for {TenantId}", tenant.TenantId);
            var tenantContext = new TenantContext(
                tenantId: tenant.TenantId,
                tenantName: tenant.TenantName,
                connectionString: tenant.ConnectionString,
                settings: tenant.GetSettingsDictionary()
            );

            await _migrationRunner.RunMigrationsForTenantAsync(tenantContext);

            // execute recipe if provided
            //if (!string.IsNullOrWhiteSpace(dto.RecipeName) && _recipeExecutor != null)
            //{
            //    _logger.LogInformation("Executing recipe {Recipe} for tenant {TenantId}", dto.RecipeName, tenant.TenantId);
            //    await _recipeExecutor.ExecuteRecipeAsync(dto.RecipeName, tenantContext);
            //}

            return CreatedAtAction(nameof(GetTenant), new { id = tenant.TenantId }, tenant);
        }

        [HttpGet]
        public async Task<IActionResult> List() => Ok(await _repo.ListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTenant(string id)
        {
            var t = await _repo.FindByIdAsync(id);
            if (t == null) return NotFound();
            return Ok(t);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTenant(string id)
        {
            var ok = await _repo.RemoveAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
