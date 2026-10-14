namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional reorg window.</summary>
public sealed record RegionalReorgWindowView(string CodeText, string Label, string Category, string Badge, string UpdatedAt, string StatusLabel)
{
    public static RegionalReorgWindowView ToRegionalReorgWindowView(RegionalReorgWindow item)
    {
        return new RegionalReorgWindowView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Category,
            Text.Truncate(item.Badge, 80),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalReorgWindowView> ToRegionalReorgWindowViews(List<RegionalReorgWindow> items)
    {
        var views = new List<RegionalReorgWindowView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalReorgWindowView(item));
        }
        return views;
    }

    public static double SumRegionalReorgWindowView(RegionalReorgWindowView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt;
    }
}
