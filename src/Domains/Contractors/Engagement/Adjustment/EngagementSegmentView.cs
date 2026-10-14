namespace Acme.Workforce.Contractors.Engagement.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement segment.</summary>
public sealed record EngagementSegmentView(string Code, string TitleText, string Reviewer, string Summary, string Channel, string AmountText, string StatusLabel)
{
    public static EngagementSegmentView ToEngagementSegmentView(EngagementSegment item)
    {
        return new EngagementSegmentView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Reviewer,
            Text.Truncate(item.Summary, 64),
            item.Channel,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementSegmentView> ToEngagementSegmentViews(List<EngagementSegment> items)
    {
        var views = new List<EngagementSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementSegmentView(item));
        }
        return views;
    }

    public static double SumEngagementSegmentView(EngagementSegmentView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
