namespace Acme.Workforce.Assets.Refresh.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient refresh profile.</summary>
public sealed record LenientRefreshProfileView(string CodeText, string Label, string Checksum, string TeamText, string Margin, string WeightText, string Phone, string Reviewer, string DurationMinutes, string StatusLabel)
{
    public static LenientRefreshProfileView ToLenientRefreshProfileView(LenientRefreshProfile item)
    {
        return new LenientRefreshProfileView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Checksum,
            item.Team,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Reviewer,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientRefreshProfileView> ToLenientRefreshProfileViews(List<LenientRefreshProfile> items)
    {
        var views = new List<LenientRefreshProfileView>();
        foreach (var item in items)
        {
            views.Add(ToLenientRefreshProfileView(item));
        }
        return views;
    }

    public static double SumLenientRefreshProfileView(LenientRefreshProfileView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedMargin + parsedWeight + (double)parsedDurationMinutes;
    }
}
