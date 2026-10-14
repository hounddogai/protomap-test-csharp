namespace Acme.Workforce.Workplace.Desk.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a desk baseline.</summary>
public sealed record DeskBaselineView(string Code, string Title, string Retries, string UnitText, string StartsAt, string StatusLabel)
{
    public static DeskBaselineView ToDeskBaselineView(DeskBaseline item)
    {
        return new DeskBaselineView(
            item.Code,
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskBaselineView> ToDeskBaselineViews(List<DeskBaseline> items)
    {
        var views = new List<DeskBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToDeskBaselineView(item));
        }
        return views;
    }

    public static double SumDeskBaselineView(DeskBaselineView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedStartsAt;
    }
}
