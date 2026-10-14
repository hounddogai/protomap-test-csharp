namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle estimate.</summary>
public sealed record MeritCycleEstimateView(string Code, string Label, string StartsAtText, string Hours, string ChannelText, string StatusLabel)
{
    public static MeritCycleEstimateView ToMeritCycleEstimateView(MeritCycleEstimate item)
    {
        return new MeritCycleEstimateView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleEstimateView> ToMeritCycleEstimateViews(List<MeritCycleEstimate> items)
    {
        var views = new List<MeritCycleEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleEstimateView(item));
        }
        return views;
    }

    public static double SumMeritCycleEstimateView(MeritCycleEstimateView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedHours;
    }
}
