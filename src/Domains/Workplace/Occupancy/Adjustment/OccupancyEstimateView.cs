namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a occupancy estimate.</summary>
public sealed record OccupancyEstimateView(string CodeText, string Title, string LocationText, string Locale, string BatchSize, string LeaveDays, string Source, string Version, string WeightText, string Label, string Fte, string StatusLabel)
{
    public static OccupancyEstimateView ToOccupancyEstimateView(OccupancyEstimate item)
    {
        return new OccupancyEstimateView(
            item.Code,
            item.Title,
            item.Location,
            item.Locale,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyEstimateView> ToOccupancyEstimateViews(List<OccupancyEstimate> items)
    {
        var views = new List<OccupancyEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyEstimateView(item));
        }
        return views;
    }

    public static double SumOccupancyEstimateView(OccupancyEstimateView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedLeaveDays + (double)parsedVersion;
    }
}
