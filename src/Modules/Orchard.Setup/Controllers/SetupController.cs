using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orchard.Setup.Models;
using Orchard.Setup.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using System;

namespace Orchard.Setup.Controllers
{
    [Area("Orchard.Setup")]
    public class SetupController : Controller
    {
        private readonly IRecipeExecutor _recipeExecutor;
        private readonly ISetupStateService _setupState;
        private readonly ILogger<SetupController> _logger;

        public SetupController(
            IRecipeExecutor recipeExecutor,
            ISetupStateService setupState,
            ILogger<SetupController> logger)
        {
            _recipeExecutor = recipeExecutor;
            _setupState = setupState;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!await _setupState.IsSetupRequiredAsync())
            {
                // redirect to home/admin if setup already completed
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var model = new SetupModel
            {
                SiteName = "Orchard App"
            };
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SetupModel model)
        {
            if (!await _setupState.IsSetupRequiredAsync())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View("Index", model);

            try
            {
                var recipePath = Path.Combine(
                    AppContext.BaseDirectory,
                    "Modules", "Orchard.Setup", "Recipes", "SetupRecipe.json");

                var recipeJson = await System.IO.File.ReadAllTextAsync(recipePath);

                // Replace template tokens (sanitise/validate inputs before use in prod)
                recipeJson = recipeJson
                    .Replace("{{SiteName}}", model.SiteName)
                    .Replace("{{AdminUser}}", model.AdminUser)
                    .Replace("{{AdminPassword}}", model.AdminPassword)
                    .Replace("{{AdminEmail}}", model.AdminEmail);

                // Parse recipe into JObject
                var recipeJObject = Newtonsoft.Json.Linq.JObject.Parse(recipeJson);

                // Convert JObject -> Dictionary<string, object> which Orchard expects in RecipeExecutionContext.Environment
                var environmentDict = recipeJObject.ToObject<Dictionary<string, object>>()
                                      ?? new Dictionary<string, object>();

                // Build the descriptor from fields inside the recipe (optional but helpful)
                var descriptor = new RecipeDescriptor
                {
                    Name = recipeJObject.Value<string>("name") ?? "setup",
                    DisplayName = recipeJObject.Value<string>("displayName") ?? "Setup",
                    Description = recipeJObject.Value<string>("description") ?? "",
                    Tags = recipeJObject["tags"]?.ToObject<string[]>() ?? Array.Empty<string>()
                };

                // Execute the recipe (the single correct public API)
                await _recipeExecutor.ExecuteAsync(Guid.NewGuid().ToString("n"), descriptor, environmentDict, CancellationToken.None);

                // Mark setup done (persist properly in production)
                await _setupState.MarkSetupCompletedAsync();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setup failed");
                ModelState.AddModelError("", "Setup failed: " + ex.Message);
                return View("Index", model);
            }
        }


    }
}
