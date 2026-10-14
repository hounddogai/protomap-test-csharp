namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using System.Globalization;

/// <summary>How screens and exports show a manual access badge budget.</summary>
public sealed record ManualAccessBadgeBudgetView(string Code, string Title, string Limit, string Checksum, string Unit, string StatusLabel)
{
    public static ManualAccessBadgeBudgetView ToManualAccessBadgeBudgetView(ManualAccessBadgeBudget item)
    {
        return new ManualAccessBadgeBudgetView(
            item.Code,
            item.Title,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualAccessBadgeBudgetView> ToManualAccessBadgeBudgetViews(List<ManualAccessBadgeBudget> items)
    {
        var views = new List<ManualAccessBadgeBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToManualAccessBadgeBudgetView(item));
        }
        return views;
    }

    public static double SumManualAccessBadgeBudgetView(ManualAccessBadgeBudgetView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedLimit;
    }
}
