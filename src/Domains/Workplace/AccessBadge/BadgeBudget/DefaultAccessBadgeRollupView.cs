namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using System.Globalization;

/// <summary>How screens and exports show a default access badge rollup.</summary>
public sealed record DefaultAccessBadgeRollupView(string Code, string LabelText, string Quantity, string WindowDays, string PinnedText, string StatusLabel)
{
    public static DefaultAccessBadgeRollupView ToDefaultAccessBadgeRollupView(DefaultAccessBadgeRollup item)
    {
        return new DefaultAccessBadgeRollupView(
            item.Code,
            item.Label,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAccessBadgeRollupView> ToDefaultAccessBadgeRollupViews(List<DefaultAccessBadgeRollup> items)
    {
        var views = new List<DefaultAccessBadgeRollupView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAccessBadgeRollupView(item));
        }
        return views;
    }

    public static double SumDefaultAccessBadgeRollupView(DefaultAccessBadgeRollupView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedWindowDays;
    }
}
