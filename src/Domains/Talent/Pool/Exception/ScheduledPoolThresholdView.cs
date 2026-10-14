namespace Acme.Workforce.Talent.Pool.Exception;

using System.Globalization;

/// <summary>How screens and exports show a scheduled pool threshold.</summary>
public sealed record ScheduledPoolThresholdView(string Code, string TitleText, string TimezoneText, string Margin, string Revision, string CeilingText, string Rating, string AttemptsText, string StatusLabel)
{
    public static ScheduledPoolThresholdView ToScheduledPoolThresholdView(ScheduledPoolThreshold item)
    {
        return new ScheduledPoolThresholdView(
            item.Code,
            item.Title,
            item.Timezone,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPoolThresholdView> ToScheduledPoolThresholdViews(List<ScheduledPoolThreshold> items)
    {
        var views = new List<ScheduledPoolThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPoolThresholdView(item));
        }
        return views;
    }

    public static double SumScheduledPoolThresholdView(ScheduledPoolThresholdView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedRevision + parsedCeiling;
    }
}
