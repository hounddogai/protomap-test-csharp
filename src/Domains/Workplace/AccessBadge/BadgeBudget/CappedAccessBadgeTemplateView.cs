namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped access badge template.</summary>
public sealed record CappedAccessBadgeTemplateView(string Code, string Title, string CeilingText, string Threshold, string Summary, string FloorValue, string Visible, string Minimum, string Quantity, string Source, string StatusLabel)
{
    public static CappedAccessBadgeTemplateView ToCappedAccessBadgeTemplateView(CappedAccessBadgeTemplate item)
    {
        return new CappedAccessBadgeTemplateView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAccessBadgeTemplateView> ToCappedAccessBadgeTemplateViews(List<CappedAccessBadgeTemplate> items)
    {
        var views = new List<CappedAccessBadgeTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAccessBadgeTemplateView(item));
        }
        return views;
    }

    public static double SumCappedAccessBadgeTemplateView(CappedAccessBadgeTemplateView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedThreshold + parsedFloorValue;
    }
}
