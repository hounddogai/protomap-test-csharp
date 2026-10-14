namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary engagement notice.</summary>
public sealed record PrimaryEngagementNoticeView(string CodeText, string Label, string Source, string EndsAt, string PinnedText, string Rating, string RetriesText, string NameText, string AmountText, string Limit, string StatusLabel)
{
    public static PrimaryEngagementNoticeView ToPrimaryEngagementNoticeView(PrimaryEngagementNotice item)
    {
        return new PrimaryEngagementNoticeView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Source,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryEngagementNoticeView> ToPrimaryEngagementNoticeViews(List<PrimaryEngagementNotice> items)
    {
        var views = new List<PrimaryEngagementNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryEngagementNoticeView(item));
        }
        return views;
    }

    public static double SumPrimaryEngagementNoticeView(PrimaryEngagementNoticeView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedRating + (double)parsedRetries;
    }
}
