namespace Acme.Workforce.Orgdesign.Span.Review;

using System.Globalization;

/// <summary>How screens and exports show a regional span record.</summary>
public sealed record RegionalSpanRecordView(string Code, string Title, string Revision, string RatioText, string Reviewer, string BatchSize, string Version, string Channel, string UpdatedAtText, string StatusLabel)
{
    public static RegionalSpanRecordView ToRegionalSpanRecordView(RegionalSpanRecord item)
    {
        return new RegionalSpanRecordView(
            item.Code,
            item.Title,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalSpanRecordView> ToRegionalSpanRecordViews(List<RegionalSpanRecord> items)
    {
        var views = new List<RegionalSpanRecordView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalSpanRecordView(item));
        }
        return views;
    }

    public static double SumRegionalSpanRecordView(RegionalSpanRecordView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedRatio + (double)parsedBatchSize;
    }
}
