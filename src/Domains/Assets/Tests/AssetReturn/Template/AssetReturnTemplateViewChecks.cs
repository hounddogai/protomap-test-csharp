namespace Acme.Workforce.Assets.Tests.AssetReturn.Template;

using Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AssetReturnTemplateViewChecks
{
    public static void AssetReturnTemplateViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnTemplate.NewAssetReturnTemplate("code-237", "label-543");
        var view = AssetReturnTemplateView.ToAssetReturnTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnTemplateView.ToAssetReturnTemplateViews(new List<AssetReturnTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnTemplateViewShowsTheStatusInCapitals();
    }
}
