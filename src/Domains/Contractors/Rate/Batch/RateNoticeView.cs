namespace Acme.Workforce.Contractors.Rate.Batch;

using System.Globalization;

/// <summary>How screens and exports show a rate notice.</summary>
public sealed record RateNoticeView(string Code, string LabelText, string ExpiresAt, string WindowDays, string JobCodeText, string Budget, string PinnedText, string Phone, string BadgeText, string StatusLabel)
{
    public static RateNoticeView ToRateNoticeView(RateNotice item)
    {
        return new RateNoticeView(
            item.Code,
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Phone,
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateNoticeView> ToRateNoticeViews(List<RateNotice> items)
    {
        var views = new List<RateNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToRateNoticeView(item));
        }
        return views;
    }

    public static double SumRateNoticeView(RateNoticeView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedWindowDays + parsedBudget;
    }
}
