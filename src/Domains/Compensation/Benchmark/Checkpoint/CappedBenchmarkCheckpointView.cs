namespace Acme.Workforce.Compensation.Benchmark.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped benchmark checkpoint.</summary>
public sealed record CappedBenchmarkCheckpointView(string CodeText, string Name, string RateText, string Category, string VersionText, string HoursText, string CostCenter, string Notes, string DiscountText, string ManagerId, string Channel, string StatusLabel)
{
    public static CappedBenchmarkCheckpointView ToCappedBenchmarkCheckpointView(CappedBenchmarkCheckpoint item)
    {
        return new CappedBenchmarkCheckpointView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Notes,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 80),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedBenchmarkCheckpointView> ToCappedBenchmarkCheckpointViews(List<CappedBenchmarkCheckpoint> items)
    {
        var views = new List<CappedBenchmarkCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToCappedBenchmarkCheckpointView(item));
        }
        return views;
    }

    public static double SumCappedBenchmarkCheckpointView(CappedBenchmarkCheckpointView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedVersion + parsedHours;
    }
}
