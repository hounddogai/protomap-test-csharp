namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment exception.</summary>
public sealed record SentimentExceptionView(string CodeText, string Label, string GradeText, string Approved, string Maximum, string TitleText, string StatusLabel)
{
    public static SentimentExceptionView ToSentimentExceptionView(SentimentException item)
    {
        return new SentimentExceptionView(
            item.Code,
            item.Label,
            Text.Truncate(item.Grade, 64),
            $"{item.Approved}",
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Title, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentExceptionView> ToSentimentExceptionViews(List<SentimentException> items)
    {
        var views = new List<SentimentExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentExceptionView(item));
        }
        return views;
    }

    public static double SumSentimentExceptionView(SentimentExceptionView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
