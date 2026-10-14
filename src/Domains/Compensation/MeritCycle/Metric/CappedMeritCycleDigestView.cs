namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped merit cycle digest.</summary>
public sealed record CappedMeritCycleDigestView(string Code, string Title, string Timezone, string Attempts, string GraceDays, string RatioText, string StatusLabel)
{
    public static CappedMeritCycleDigestView ToCappedMeritCycleDigestView(CappedMeritCycleDigest item)
    {
        return new CappedMeritCycleDigestView(
            item.Code,
            item.Title,
            Text.Truncate(item.Timezone, 80),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedMeritCycleDigestView> ToCappedMeritCycleDigestViews(List<CappedMeritCycleDigest> items)
    {
        var views = new List<CappedMeritCycleDigestView>();
        foreach (var item in items)
        {
            views.Add(ToCappedMeritCycleDigestView(item));
        }
        return views;
    }

    public static double SumCappedMeritCycleDigestView(CappedMeritCycleDigestView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedGraceDays + parsedRatio;
    }
}
