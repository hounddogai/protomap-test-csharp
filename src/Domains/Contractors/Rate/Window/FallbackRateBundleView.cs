namespace Acme.Workforce.Contractors.Rate.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback rate bundle.</summary>
public sealed record FallbackRateBundleView(string Code, string TitleText, string EndsAt, string ChecksumText, string ManagerId, string Team, string WeightText, string Attempts, string Minimum, string Active, string StatusLabel)
{
    public static FallbackRateBundleView ToFallbackRateBundleView(FallbackRateBundle item)
    {
        return new FallbackRateBundleView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            Text.Truncate(item.ManagerId, 64),
            item.Team,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackRateBundleView> ToFallbackRateBundleViews(List<FallbackRateBundle> items)
    {
        var views = new List<FallbackRateBundleView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackRateBundleView(item));
        }
        return views;
    }

    public static double SumFallbackRateBundleView(FallbackRateBundleView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedWeight + (double)parsedAttempts;
    }
}
