namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped career path schedule.</summary>
public sealed record CappedCareerPathScheduleView(string Code, string Label, string WindowDays, string Locale, string Currency, string NameText, string TimezoneText, string ScoreText, string Sequence, string Country, string ReviewerText, string StatusLabel)
{
    public static CappedCareerPathScheduleView ToCappedCareerPathScheduleView(CappedCareerPathSchedule item)
    {
        return new CappedCareerPathScheduleView(
            item.Code,
            item.Label,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 64),
            Text.Truncate(item.Currency, 80),
            item.Name,
            Text.Truncate(item.Timezone, 80),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 80),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedCareerPathScheduleView> ToCappedCareerPathScheduleViews(List<CappedCareerPathSchedule> items)
    {
        var views = new List<CappedCareerPathScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToCappedCareerPathScheduleView(item));
        }
        return views;
    }

    public static double SumCappedCareerPathScheduleView(CappedCareerPathScheduleView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedScore + (double)parsedSequence;
    }
}
