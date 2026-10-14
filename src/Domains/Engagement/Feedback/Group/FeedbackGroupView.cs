namespace Acme.Workforce.Engagement.Feedback.Group;

using System.Globalization;

/// <summary>How screens and exports show a feedback group.</summary>
public sealed record FeedbackGroupView(string CodeText, string Name, string Category, string ExpiresAtText, string Capacity, string Checksum, string Amount, string Title, string Version, string StatusLabel)
{
    public static FeedbackGroupView ToFeedbackGroupView(FeedbackGroup item)
    {
        return new FeedbackGroupView(
            item.Code,
            item.Name,
            item.Category,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackGroupView> ToFeedbackGroupViews(List<FeedbackGroup> items)
    {
        var views = new List<FeedbackGroupView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackGroupView(item));
        }
        return views;
    }

    public static double SumFeedbackGroupView(FeedbackGroupView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedCapacity + parsedAmount;
    }
}
