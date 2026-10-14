namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted mobility rollup.</summary>
public sealed record WeightedMobilityRollupView(string Code, string Name, string ContactEmail, string Archived, string Description, string Threshold, string Version, string StatusLabel)
{
    public static WeightedMobilityRollupView ToWeightedMobilityRollupView(WeightedMobilityRollup item)
    {
        return new WeightedMobilityRollupView(
            item.Code,
            item.Name,
            Text.Truncate(item.ContactEmail, 120),
            $"{item.Archived}",
            item.Description,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedMobilityRollupView> ToWeightedMobilityRollupViews(List<WeightedMobilityRollup> items)
    {
        var views = new List<WeightedMobilityRollupView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedMobilityRollupView(item));
        }
        return views;
    }

    public static double SumWeightedMobilityRollupView(WeightedMobilityRollupView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedVersion;
    }
}
