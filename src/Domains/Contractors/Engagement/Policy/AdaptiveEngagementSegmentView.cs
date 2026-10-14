namespace Acme.Workforce.Contractors.Engagement.Policy;

using System.Globalization;

/// <summary>How screens and exports show a adaptive engagement segment.</summary>
public sealed record AdaptiveEngagementSegmentView(string CodeText, string Label, string EndsAt, string TitleText, string UnitText, string Maximum, string JobCodeText, string StatusLabel)
{
    public static AdaptiveEngagementSegmentView ToAdaptiveEngagementSegmentView(AdaptiveEngagementSegment item)
    {
        return new AdaptiveEngagementSegmentView(
            item.Code,
            item.Label,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Unit,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveEngagementSegmentView> ToAdaptiveEngagementSegmentViews(List<AdaptiveEngagementSegment> items)
    {
        var views = new List<AdaptiveEngagementSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveEngagementSegmentView(item));
        }
        return views;
    }

    public static double SumAdaptiveEngagementSegmentView(AdaptiveEngagementSegmentView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedMaximum;
    }
}
