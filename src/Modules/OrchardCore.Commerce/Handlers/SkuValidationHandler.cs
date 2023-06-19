﻿using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Handlers;

public class SkuValidationHandler : ContentPartHandler<ProductPart>
{
    private readonly ISession _session;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    public SkuValidationHandler(
        ISession session,
        IUpdateModelAccessor updateModelAccessor)
    {
        _session = session;
        _updateModelAccessor = updateModelAccessor;
    }

    public override async Task UpdatedAsync(UpdateContentContext context, ProductPart part)
    {
        if (string.IsNullOrWhiteSpace(part.Sku))
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(nameof(part.Sku), "SKU must not be empty.");
            return;
        }

        var isProductSkuAlreadyExisting = await _session
            .Query<ContentItem, ProductPartIndex>(index =>
                index.Sku == part.Sku &&
                index.ContentItemId != part.ContentItem.ContentItemId)
            .CountAsync() > 0;

        if (isProductSkuAlreadyExisting)
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(part.Sku),
                "SKU must be unique. A product with the given SKU already exists.");
        }
    }
}
