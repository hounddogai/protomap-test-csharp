namespace Acme.Workforce.Engagement.Feedback.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback slot.</summary>
public sealed record FeedbackSlotView(string Code, string Name, string ChannelText, string Limit, string Maximum, string Title, string CostCenter, string StatusLabel)
{
    public static FeedbackSlotView ToFeedbackSlotView(FeedbackSlot item)
    {
        return new FeedbackSlotView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Channel,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackSlotView> ToFeedbackSlotViews(List<FeedbackSlot> items)
    {
        var views = new List<FeedbackSlotView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackSlotView(item));
        }
        return views;
    }

    public static double SumFeedbackSlotView(FeedbackSlotView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedMaximum;
    }
}
