namespace Acme.Workforce.Assets.Tests.Warranty.Template;

using Acme.Workforce.Assets.Warranty.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WarrantyTemplateViewChecks
{
    public static void WarrantyTemplateViewShowsTheStatusInCapitals()
    {
        var item = WarrantyTemplate.NewWarrantyTemplate("code-527", "name-588");
        var view = WarrantyTemplateView.ToWarrantyTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyTemplateView.ToWarrantyTemplateViews(new List<WarrantyTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyTemplateViewShowsTheStatusInCapitals();
    }
}
