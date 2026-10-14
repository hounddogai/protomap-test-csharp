namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using System.Globalization;

/// <summary>How screens and exports show a adaptive cost model checkpoint.</summary>
public sealed record AdaptiveCostModelCheckpointView(string Code, string Name, string Archived, string WindowDays, string GraceDays, string Minimum, string Unit, string StatusLabel)
{
    public static AdaptiveCostModelCheckpointView ToAdaptiveCostModelCheckpointView(AdaptiveCostModelCheckpoint item)
    {
        return new AdaptiveCostModelCheckpointView(
            item.Code,
            item.Name,
            $"{item.Archived}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveCostModelCheckpointView> ToAdaptiveCostModelCheckpointViews(List<AdaptiveCostModelCheckpoint> items)
    {
        var views = new List<AdaptiveCostModelCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveCostModelCheckpointView(item));
        }
        return views;
    }

    public static double SumAdaptiveCostModelCheckpointView(AdaptiveCostModelCheckpointView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedGraceDays + parsedMinimum;
    }
}
