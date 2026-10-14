namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped pay run record.</summary>
public sealed record CappedPayRunRecordView(string CodeText, string Name, string Retries, string JobCode, string Margin, string Timezone, string ScoreText, string Title, string TenureMonths, string HoursText, string StatusLabel)
{
    public static CappedPayRunRecordView ToCappedPayRunRecordView(CappedPayRunRecord item)
    {
        return new CappedPayRunRecordView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 120),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPayRunRecordView> ToCappedPayRunRecordViews(List<CappedPayRunRecord> items)
    {
        var views = new List<CappedPayRunRecordView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPayRunRecordView(item));
        }
        return views;
    }

    public static double SumCappedPayRunRecordView(CappedPayRunRecordView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedMargin + parsedScore;
    }
}
