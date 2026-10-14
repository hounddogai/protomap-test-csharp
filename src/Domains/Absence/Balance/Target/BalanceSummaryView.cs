namespace Acme.Workforce.Absence.Balance.Target;

using System.Globalization;

/// <summary>How screens and exports show a balance summary.</summary>
public sealed record BalanceSummaryView(string Code, string Name, string Checksum, string Label, string Limit, string Maximum, string StatusLabel)
{
    public static BalanceSummaryView ToBalanceSummaryView(BalanceSummary item)
    {
        return new BalanceSummaryView(
            item.Code,
            item.Name,
            item.Checksum,
            item.Label,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceSummaryView> ToBalanceSummaryViews(List<BalanceSummary> items)
    {
        var views = new List<BalanceSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceSummaryView(item));
        }
        return views;
    }

    public static double SumBalanceSummaryView(BalanceSummaryView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedMaximum;
    }
}
