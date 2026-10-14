namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using System.Globalization;

/// <summary>How screens and exports show a manual garnishment baseline.</summary>
public sealed record ManualGarnishmentBaselineView(string Code, string Name, string Fte, string Minimum, string Rate, string StatusLabel)
{
    public static ManualGarnishmentBaselineView ToManualGarnishmentBaselineView(ManualGarnishmentBaseline item)
    {
        return new ManualGarnishmentBaselineView(
            item.Code,
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualGarnishmentBaselineView> ToManualGarnishmentBaselineViews(List<ManualGarnishmentBaseline> items)
    {
        var views = new List<ManualGarnishmentBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToManualGarnishmentBaselineView(item));
        }
        return views;
    }

    public static double SumManualGarnishmentBaselineView(ManualGarnishmentBaselineView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedFte + parsedMinimum + parsedRate;
    }
}
