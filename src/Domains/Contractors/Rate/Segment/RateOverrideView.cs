namespace Acme.Workforce.Contractors.Rate.Segment;

using System.Globalization;

/// <summary>How screens and exports show a rate override.</summary>
public sealed record RateOverrideView(string Code, string Name, string ScoreText, string Description, string Locale, string HoursText, string Source, string AttemptsText, string Pinned, string StatusLabel)
{
    public static RateOverrideView ToRateOverrideView(RateOverride item)
    {
        return new RateOverrideView(
            item.Code,
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.Locale,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateOverrideView> ToRateOverrideViews(List<RateOverride> items)
    {
        var views = new List<RateOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToRateOverrideView(item));
        }
        return views;
    }

    public static double SumRateOverrideView(RateOverrideView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return parsedScore + parsedHours + (double)parsedAttempts;
    }
}
