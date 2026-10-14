namespace Acme.Workforce.Contractors.Rate.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate record.</summary>
public sealed record RateRecordView(string CodeText, string Title, string Channel, string Country, string ScoreText, string Currency, string Limit, string Segment, string StatusLabel)
{
    public static RateRecordView ToRateRecordView(RateRecord item)
    {
        return new RateRecordView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Channel,
            item.Country,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 64),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateRecordView> ToRateRecordViews(List<RateRecord> items)
    {
        var views = new List<RateRecordView>();
        foreach (var item in items)
        {
            views.Add(ToRateRecordView(item));
        }
        return views;
    }

    public static double SumRateRecordView(RateRecordView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedLimit;
    }
}
