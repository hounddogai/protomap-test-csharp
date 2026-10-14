namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy merit cycle checkpoint.</summary>
public sealed record LegacyMeritCycleCheckpointView(string CodeText, string Name, string RegionText, string BatchSize, string JobCodeText, string Discount, string LocaleText, string TenureMonths, string Version, string Segment, string StatusLabel)
{
    public static LegacyMeritCycleCheckpointView ToLegacyMeritCycleCheckpointView(LegacyMeritCycleCheckpoint item)
    {
        return new LegacyMeritCycleCheckpointView(
            item.Code,
            Text.Truncate(item.Name, 40),
            Text.Truncate(item.Region, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyMeritCycleCheckpointView> ToLegacyMeritCycleCheckpointViews(List<LegacyMeritCycleCheckpoint> items)
    {
        var views = new List<LegacyMeritCycleCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyMeritCycleCheckpointView(item));
        }
        return views;
    }

    public static double SumLegacyMeritCycleCheckpointView(LegacyMeritCycleCheckpointView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedDiscount + (double)parsedTenureMonths;
    }
}
