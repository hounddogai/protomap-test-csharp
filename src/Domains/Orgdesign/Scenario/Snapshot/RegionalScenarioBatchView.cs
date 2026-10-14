namespace Acme.Workforce.Orgdesign.Scenario.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional scenario batch.</summary>
public sealed record RegionalScenarioBatchView(string CodeText, string Label, string Retries, string Maximum, string CapacityText, string CurrencyText, string RegionText, string DurationMinutes, string BatchSize, string Name, string StatusLabel)
{
    public static RegionalScenarioBatchView ToRegionalScenarioBatchView(RegionalScenarioBatch item)
    {
        return new RegionalScenarioBatchView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.Region,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalScenarioBatchView> ToRegionalScenarioBatchViews(List<RegionalScenarioBatch> items)
    {
        var views = new List<RegionalScenarioBatchView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalScenarioBatchView(item));
        }
        return views;
    }

    public static double SumRegionalScenarioBatchView(RegionalScenarioBatchView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedMaximum + (double)parsedCapacity;
    }
}
