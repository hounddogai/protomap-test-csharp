namespace Acme.Workforce.Compensation.PayBand.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band summary.</summary>
public sealed record PayBandSummaryView(string CodeText, string TitleText, string Category, string Rate, string TenureMonths, string CostCenterText, string ManagerIdText, string Revision, string JobCode, string Ratio, string StatusLabel)
{
    public static PayBandSummaryView ToPayBandSummaryView(PayBandSummary item)
    {
        return new PayBandSummaryView(
            item.Code,
            item.Title,
            item.Category,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 40),
            Text.Truncate(item.ManagerId, 120),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandSummaryView> ToPayBandSummaryViews(List<PayBandSummary> items)
    {
        var views = new List<PayBandSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandSummaryView(item));
        }
        return views;
    }

    public static double SumPayBandSummaryView(PayBandSummaryView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedTenureMonths + (double)parsedRevision;
    }
}
