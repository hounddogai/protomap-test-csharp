namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a equity grant record.</summary>
public sealed record EquityGrantRecordView(string CodeText, string Label, string EndsAt, string Ratio, string Score, string ContactEmail, string StartsAtText, string StatusLabel)
{
    public static EquityGrantRecordView ToEquityGrantRecordView(EquityGrantRecord item)
    {
        return new EquityGrantRecordView(
            item.Code,
            item.Label,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantRecordView> ToEquityGrantRecordViews(List<EquityGrantRecord> items)
    {
        var views = new List<EquityGrantRecordView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantRecordView(item));
        }
        return views;
    }

    public static double SumEquityGrantRecordView(EquityGrantRecordView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedRatio + parsedScore;
    }
}
