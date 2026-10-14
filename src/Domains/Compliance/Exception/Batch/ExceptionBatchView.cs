namespace Acme.Workforce.Compliance.Exception.Batch;

using System.Globalization;

/// <summary>How screens and exports show a exception batch.</summary>
public sealed record ExceptionBatchView(string Code, string TitleText, string Rate, string CurrencyText, string Team, string ApprovedText, string JobCode, string MinimumText, string Score, string Sequence, string Summary, string StatusLabel)
{
    public static ExceptionBatchView ToExceptionBatchView(ExceptionBatch item)
    {
        return new ExceptionBatchView(
            item.Code,
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Team,
            $"{item.Approved}",
            item.JobCode,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionBatchView> ToExceptionBatchViews(List<ExceptionBatch> items)
    {
        var views = new List<ExceptionBatchView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionBatchView(item));
        }
        return views;
    }

    public static double SumExceptionBatchView(ExceptionBatchView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedRate + parsedMinimum + parsedScore;
    }
}
