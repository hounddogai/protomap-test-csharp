namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment rollup.</summary>
public sealed record GarnishmentRollupView(string CodeText, string Label, string Notes, string Title, string RateText, string Version, string BatchSizeText, string Reviewer, string Summary, string CapacityText, string StatusLabel)
{
    public static GarnishmentRollupView ToGarnishmentRollupView(GarnishmentRollup item)
    {
        return new GarnishmentRollupView(
            item.Code,
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.Notes, 40),
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Summary,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentRollupView> ToGarnishmentRollupViews(List<GarnishmentRollup> items)
    {
        var views = new List<GarnishmentRollupView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentRollupView(item));
        }
        return views;
    }

    public static double SumGarnishmentRollupView(GarnishmentRollupView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedVersion + (double)parsedBatchSize;
    }
}
