namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge mapping.</summary>
public sealed record AccessBadgeMappingView(string CodeText, string Name, string CategoryText, string Label, string Limit, string FloorValue, string Title, string Summary, string CeilingText, string Currency, string Country, string StatusLabel)
{
    public static AccessBadgeMappingView ToAccessBadgeMappingView(AccessBadgeMapping item)
    {
        return new AccessBadgeMappingView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Category,
            Text.Truncate(item.Label, 80),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.Summary,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            Text.Truncate(item.Country, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeMappingView> ToAccessBadgeMappingViews(List<AccessBadgeMapping> items)
    {
        var views = new List<AccessBadgeMappingView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeMappingView(item));
        }
        return views;
    }

    public static double SumAccessBadgeMappingView(AccessBadgeMappingView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedFloorValue + parsedCeiling;
    }
}
