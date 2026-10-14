namespace Acme.Workforce.Orgdesign.CostModel.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a cost model checkpoint.</summary>
public sealed record CostModelCheckpointView(string Code, string Name, string TitleText, string VersionText, string TimezoneText, string DurationMinutes, string RetriesText, string Score, string StatusLabel)
{
    public static CostModelCheckpointView ToCostModelCheckpointView(CostModelCheckpoint item)
    {
        return new CostModelCheckpointView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Title,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelCheckpointView> ToCostModelCheckpointViews(List<CostModelCheckpoint> items)
    {
        var views = new List<CostModelCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelCheckpointView(item));
        }
        return views;
    }

    public static double SumCostModelCheckpointView(CostModelCheckpointView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedDurationMinutes + (double)parsedRetries;
    }
}
