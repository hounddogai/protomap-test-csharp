namespace Acme.Workforce.Compensation.Adjustment.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback adjustment checkpoint.</summary>
public sealed record FallbackAdjustmentCheckpointView(string CodeText, string Name, string LimitText, string Revision, string Active, string StatusLabel)
{
    public static FallbackAdjustmentCheckpointView ToFallbackAdjustmentCheckpointView(FallbackAdjustmentCheckpoint item)
    {
        return new FallbackAdjustmentCheckpointView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackAdjustmentCheckpointView> ToFallbackAdjustmentCheckpointViews(List<FallbackAdjustmentCheckpoint> items)
    {
        var views = new List<FallbackAdjustmentCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackAdjustmentCheckpointView(item));
        }
        return views;
    }

    public static double SumFallbackAdjustmentCheckpointView(FallbackAdjustmentCheckpointView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedRevision;
    }
}
