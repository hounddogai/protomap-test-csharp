namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient merit cycle claim.</summary>
public sealed record LenientMeritCycleClaimView(string CodeText, string Title, string Pinned, string Channel, string Retries, string Rate, string StatusLabel)
{
    public static LenientMeritCycleClaimView ToLenientMeritCycleClaimView(LenientMeritCycleClaim item)
    {
        return new LenientMeritCycleClaimView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 64),
            $"{item.Pinned}",
            Text.Truncate(item.Channel, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientMeritCycleClaimView> ToLenientMeritCycleClaimViews(List<LenientMeritCycleClaim> items)
    {
        var views = new List<LenientMeritCycleClaimView>();
        foreach (var item in items)
        {
            views.Add(ToLenientMeritCycleClaimView(item));
        }
        return views;
    }

    public static double SumLenientMeritCycleClaimView(LenientMeritCycleClaimView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedRate;
    }
}
