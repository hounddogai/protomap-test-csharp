namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment notice.</summary>
public sealed record SentimentNoticeView(string CodeText, string Label, string MinimumText, string ChannelText, string Badge, string TenureMonthsText, string StatusLabel)
{
    public static SentimentNoticeView ToSentimentNoticeView(SentimentNotice item)
    {
        return new SentimentNoticeView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            Text.Truncate(item.Badge, 120),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentNoticeView> ToSentimentNoticeViews(List<SentimentNotice> items)
    {
        var views = new List<SentimentNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentNoticeView(item));
        }
        return views;
    }

    public static double SumSentimentNoticeView(SentimentNoticeView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedTenureMonths;
    }
}
