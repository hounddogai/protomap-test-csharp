namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal garnishment notice.</summary>
public sealed record SeasonalGarnishmentNoticeView(string CodeText, string Label, string TenureMonths, string EmployeeId, string Margin, string JobCodeText, string Retries, string CategoryText, string StatusLabel)
{
    public static SeasonalGarnishmentNoticeView ToSeasonalGarnishmentNoticeView(SeasonalGarnishmentNotice item)
    {
        return new SeasonalGarnishmentNoticeView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 120),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 64),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalGarnishmentNoticeView> ToSeasonalGarnishmentNoticeViews(List<SeasonalGarnishmentNotice> items)
    {
        var views = new List<SeasonalGarnishmentNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalGarnishmentNoticeView(item));
        }
        return views;
    }

    public static double SumSeasonalGarnishmentNoticeView(SeasonalGarnishmentNoticeView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedMargin + (double)parsedRetries;
    }
}
