namespace Acme.Workforce.Talent.Nomination.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination notice.</summary>
public sealed record NominationNoticeView(string Code, string Title, string Discount, string OwnerIdText, string Location, string Reviewer, string Currency, string AttemptsText, string SegmentText, string StatusLabel)
{
    public static NominationNoticeView ToNominationNoticeView(NominationNotice item)
    {
        return new NominationNoticeView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            Text.Truncate(item.Location, 80),
            Text.Truncate(item.Reviewer, 40),
            item.Currency,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationNoticeView> ToNominationNoticeViews(List<NominationNotice> items)
    {
        var views = new List<NominationNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToNominationNoticeView(item));
        }
        return views;
    }

    public static double SumNominationNoticeView(NominationNoticeView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedAttempts;
    }
}
