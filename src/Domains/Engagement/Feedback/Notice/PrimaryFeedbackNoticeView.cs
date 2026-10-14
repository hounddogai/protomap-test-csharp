namespace Acme.Workforce.Engagement.Feedback.Notice;

using System.Globalization;

/// <summary>How screens and exports show a primary feedback notice.</summary>
public sealed record PrimaryFeedbackNoticeView(string Code, string Title, string TenureMonthsText, string FloorValue, string Limit, string StatusLabel)
{
    public static PrimaryFeedbackNoticeView ToPrimaryFeedbackNoticeView(PrimaryFeedbackNotice item)
    {
        return new PrimaryFeedbackNoticeView(
            item.Code,
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryFeedbackNoticeView> ToPrimaryFeedbackNoticeViews(List<PrimaryFeedbackNotice> items)
    {
        var views = new List<PrimaryFeedbackNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryFeedbackNoticeView(item));
        }
        return views;
    }

    public static double SumPrimaryFeedbackNoticeView(PrimaryFeedbackNoticeView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedFloorValue + (double)parsedLimit;
    }
}
