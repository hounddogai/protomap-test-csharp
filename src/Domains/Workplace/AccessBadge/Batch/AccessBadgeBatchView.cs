namespace Acme.Workforce.Workplace.AccessBadge.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge batch.</summary>
public sealed record AccessBadgeBatchView(string CodeText, string NameText, string SalaryBandText, string RateText, string Channel, string Badge, string GraceDays, string TenureMonths, string EmployeeIdText, string StatusLabel)
{
    public static AccessBadgeBatchView ToAccessBadgeBatchView(AccessBadgeBatch item)
    {
        return new AccessBadgeBatchView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.SalaryBand,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 64),
            Text.Truncate(item.Badge, 40),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeBatchView> ToAccessBadgeBatchViews(List<AccessBadgeBatch> items)
    {
        var views = new List<AccessBadgeBatchView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeBatchView(item));
        }
        return views;
    }

    public static double SumAccessBadgeBatchView(AccessBadgeBatchView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedGraceDays + (double)parsedTenureMonths;
    }
}
