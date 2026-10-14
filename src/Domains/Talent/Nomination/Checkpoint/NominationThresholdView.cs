namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination threshold.</summary>
public sealed record NominationThresholdView(string CodeText, string Label, string EndsAt, string Skill, string Segment, string Location, string Rating, string WindowDays, string StatusLabel)
{
    public static NominationThresholdView ToNominationThresholdView(NominationThreshold item)
    {
        return new NominationThresholdView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            Text.Truncate(item.Segment, 120),
            item.Location,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationThresholdView> ToNominationThresholdViews(List<NominationThreshold> items)
    {
        var views = new List<NominationThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToNominationThresholdView(item));
        }
        return views;
    }

    public static double SumNominationThresholdView(NominationThresholdView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedRating + (double)parsedWindowDays;
    }
}
