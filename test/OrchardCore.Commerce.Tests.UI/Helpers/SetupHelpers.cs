﻿using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;

namespace OrchardCore.Commerce.Tests.UI.Helpers;

public static class SetupHelpers
{
    public const string RecipeId = "OrchardCore.Commerce.Setup";

    public static async Task<Uri> RunSetupAsync(UITestContext context)
    {
        var homepageUri = await context.GoToSetupPageAndSetupOrchardCoreAsync(
            new OrchardCoreSetupParameters(context)
            {
                SiteName = "Orchard Core Commerce",
                RecipeId = RecipeId,
                TablePrefix = "oc",
                SiteTimeZoneValue = "Europe/London",
            });

        context.Exists(By.Id("mainNav"));

        return homepageUri;
    }
}
