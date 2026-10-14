namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using System.Globalization;

/// <summary>How screens and exports show a adaptive retro pay checkpoint.</summary>
public sealed record AdaptiveRetroPayCheckpointView(string Code, string Name, string Cost, string ManagerId, string Limit, string Threshold, string StatusLabel)
{
    public static AdaptiveRetroPayCheckpointView ToAdaptiveRetroPayCheckpointView(AdaptiveRetroPayCheckpoint item)
    {
        return new AdaptiveRetroPayCheckpointView(
            item.Code,
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveRetroPayCheckpointView> ToAdaptiveRetroPayCheckpointViews(List<AdaptiveRetroPayCheckpoint> items)
    {
        var views = new List<AdaptiveRetroPayCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveRetroPayCheckpointView(item));
        }
        return views;
    }

    public static double SumAdaptiveRetroPayCheckpointView(AdaptiveRetroPayCheckpointView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedLimit + parsedThreshold;
    }
}
