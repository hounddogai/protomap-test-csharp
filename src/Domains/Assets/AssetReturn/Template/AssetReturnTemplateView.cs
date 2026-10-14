namespace Acme.Workforce.Assets.AssetReturn.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return template.</summary>
public sealed record AssetReturnTemplateView(string Code, string LabelText, string CreatedAtText, string DescriptionText, string Segment, string Revision, string SummaryText, string DurationMinutes, string StatusLabel)
{
    public static AssetReturnTemplateView ToAssetReturnTemplateView(AssetReturnTemplate item)
    {
        return new AssetReturnTemplateView(
            item.Code,
            item.Label,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 40),
            Text.Truncate(item.Segment, 40),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 80),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnTemplateView> ToAssetReturnTemplateViews(List<AssetReturnTemplate> items)
    {
        var views = new List<AssetReturnTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnTemplateView(item));
        }
        return views;
    }

    public static double SumAssetReturnTemplateView(AssetReturnTemplateView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedRevision + (double)parsedDurationMinutes;
    }
}
