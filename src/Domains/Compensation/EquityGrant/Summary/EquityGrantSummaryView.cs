namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using System.Globalization;

/// <summary>How screens and exports show a equity grant summary.</summary>
public sealed record EquityGrantSummaryView(string Code, string Label, string Amount, string Unit, string Checksum, string HoursText, string Ceiling, string StatusLabel)
{
    public static EquityGrantSummaryView ToEquityGrantSummaryView(EquityGrantSummary item)
    {
        return new EquityGrantSummaryView(
            item.Code,
            item.Label,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.Checksum,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantSummaryView> ToEquityGrantSummaryViews(List<EquityGrantSummary> items)
    {
        var views = new List<EquityGrantSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantSummaryView(item));
        }
        return views;
    }

    public static double SumEquityGrantSummaryView(EquityGrantSummaryView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedAmount + parsedHours + parsedCeiling;
    }
}
