namespace Acme.Workforce.Compliance.Exception.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception record.</summary>
public sealed record ExceptionRecordView(string CodeText, string Title, string Name, string Score, string RateText, string Approved, string Cost, string StatusLabel)
{
    public static ExceptionRecordView ToExceptionRecordView(ExceptionRecord item)
    {
        return new ExceptionRecordView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionRecordView> ToExceptionRecordViews(List<ExceptionRecord> items)
    {
        var views = new List<ExceptionRecordView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionRecordView(item));
        }
        return views;
    }

    public static double SumExceptionRecordView(ExceptionRecordView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedScore + parsedRate + parsedCost;
    }
}
