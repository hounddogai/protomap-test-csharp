namespace Acme.Workforce.Contractors.Rate.RateNotice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global rate exception.</summary>
public sealed record GlobalRateExceptionView(string Code, string Label, string Checksum, string Region, string EmployeeId, string Currency, string Score, string StatusLabel)
{
    public static GlobalRateExceptionView ToGlobalRateExceptionView(GlobalRateException item)
    {
        return new GlobalRateExceptionView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Checksum,
            item.Region,
            item.EmployeeId,
            Text.Truncate(item.Currency, 80),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalRateExceptionView> ToGlobalRateExceptionViews(List<GlobalRateException> items)
    {
        var views = new List<GlobalRateExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalRateExceptionView(item));
        }
        return views;
    }

    public static double SumGlobalRateExceptionView(GlobalRateExceptionView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
