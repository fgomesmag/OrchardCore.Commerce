using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement;
using OrchardCore.Templates.Controllers;
using OrchardCore.Templates.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class FieldsOnlyDisplayManager : IFieldsOnlyDisplayManager
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IHttpContextAccessor _hca;
    private readonly IShapeFactory _shapeFactory;
    private readonly TemplatesManager _templatesManager;

    public FieldsOnlyDisplayManager(
        IContentDefinitionManager contentDefinitionManager,
        IHttpContextAccessor hca,
        IShapeFactory shapeFactory,
        TemplatesManager templatesManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _hca = hca;
        _shapeFactory = shapeFactory;
        _templatesManager = templatesManager;
    }

    public IEnumerable<string> GetFieldShapeTypes(
        ContentItem contentItem,
        string displayType = CommonContentDisplayTypes.Detail)
    {
        var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
        return typeDefinition
            .Parts
            .SelectMany(part =>
                part.PartDefinition.Fields.Select(field => new
                {
                    PartName = part.Name,
                    FieldName = field.Name,
                    PartOrder = GetNumericOrder(part),
                    FieldOrder = GetNumericOrder(field),
                }))
            .OrderBy(item => item.PartOrder)
            .ThenBy(item => item.FieldOrder)
            .Select(item => $"{contentItem.ContentType}_{displayType}__{item.PartName}__{item.FieldName}");
    }

    public async Task<IEnumerable<IShape>> DisplayFieldsAsync(
        ContentItem contentItem,
        string displayType = CommonContentDisplayTypes.Detail) =>
        await GetFieldShapeTypes(contentItem, displayType)
            .AwaitEachAsync(async shapeType => await _shapeFactory.CreateAsync(shapeType));

    public async Task<IEnumerable<(string ShapeType, Uri Url, bool IsNew)>> GetFieldTemplateEditorUrlsAsync(
        ContentItem contentItem,
        string displayType = CommonContentDisplayTypes.Detail)
    {
        var existingTemplates = (await _templatesManager.LoadTemplatesDocumentAsync()).Templates.Keys;

        if (_hca.HttpContext is not { } context) throw new InvalidOperationException("Missing HTTP context!");
        var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;

        var editAction = context.ActionTask<TemplateController>(controller => controller.Edit(null, false, returnUrl));
        var createAction = context.ActionTask<TemplateController>(controller => controller.Create(false, returnUrl));

        return GetFieldShapeTypes(contentItem, displayType)
            .Select(name =>
            {
                var exists = existingTemplates.Contains(name);
                var url = new Uri($"{(exists ? editAction : createAction)}&name={name}");
                return (ShapeType: name, Url: url, IsNew: !exists);
            });
    }

    private int GetNumericOrder(ContentTypePartDefinition part)
    {
        var defaultPosition = _contentDefinitionManager
            .GetPartDefinition(part.PartDefinition.Name)?
            .DefaultPosition() ?? "5";
        return int.Parse(
            part.GetSettings<ContentTypePartSettings>().Position ?? defaultPosition,
            CultureInfo.InvariantCulture);
    }

    private static int GetNumericOrder(ContentPartFieldDefinition field) =>
        int.Parse(
            field.GetSettings<ContentPartFieldSettings>().Position ?? "0",
            CultureInfo.InvariantCulture);
}
