namespace Acme.Workforce.Talent.Mobility.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility preference.</summary>
public sealed record MobilityPreferenceView(string CodeText, string Title, string TimezoneText, string Badge, string TenureMonths, string ApprovedText, string SegmentText, string ScoreText, string Grade, string StatusLabel)
{
    public static MobilityPreferenceView ToMobilityPreferenceView(MobilityPreference item)
    {
        return new MobilityPreferenceView(
            item.Code,
            item.Title,
            item.Timezone,
            item.Badge,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            Text.Truncate(item.Segment, 40),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityPreferenceView> ToMobilityPreferenceViews(List<MobilityPreference> items)
    {
        var views = new List<MobilityPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityPreferenceView(item));
        }
        return views;
    }

    public static double SumMobilityPreferenceView(MobilityPreferenceView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedScore;
    }
}
