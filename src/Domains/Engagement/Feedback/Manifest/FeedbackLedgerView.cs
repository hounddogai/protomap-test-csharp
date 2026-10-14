namespace Acme.Workforce.Engagement.Feedback.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback ledger.</summary>
public sealed record FeedbackLedgerView(string Code, string Label, string Skill, string Revision, string Grade, string Timezone, string LeaveDays, string RatingText, string StatusLabel)
{
    public static FeedbackLedgerView ToFeedbackLedgerView(FeedbackLedger item)
    {
        return new FeedbackLedgerView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Skill,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            item.Timezone,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackLedgerView> ToFeedbackLedgerViews(List<FeedbackLedger> items)
    {
        var views = new List<FeedbackLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackLedgerView(item));
        }
        return views;
    }

    public static double SumFeedbackLedgerView(FeedbackLedgerView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedLeaveDays + parsedRating;
    }
}
