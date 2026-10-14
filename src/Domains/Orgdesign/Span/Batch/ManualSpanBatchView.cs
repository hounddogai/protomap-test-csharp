namespace Acme.Workforce.Orgdesign.Span.Batch;

using System.Globalization;

/// <summary>How screens and exports show a manual span batch.</summary>
public sealed record ManualSpanBatchView(string Code, string TitleText, string RatioText, string Channel, string LeaveDays, string CountryText, string UpdatedAt, string StatusLabel)
{
    public static ManualSpanBatchView ToManualSpanBatchView(ManualSpanBatch item)
    {
        return new ManualSpanBatchView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualSpanBatchView> ToManualSpanBatchViews(List<ManualSpanBatch> items)
    {
        var views = new List<ManualSpanBatchView>();
        foreach (var item in items)
        {
            views.Add(ToManualSpanBatchView(item));
        }
        return views;
    }

    public static double SumManualSpanBatchView(ManualSpanBatchView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedLeaveDays + (double)parsedUpdatedAt;
    }
}
