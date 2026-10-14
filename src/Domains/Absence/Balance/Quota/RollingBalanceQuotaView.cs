namespace Acme.Workforce.Absence.Balance.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling balance quota.</summary>
public sealed record RollingBalanceQuotaView(string CodeText, string Title, string FloorValue, string Limit, string Currency, string TenureMonthsText, string ArchivedText, string Unit, string EndsAt, string StatusLabel)
{
    public static RollingBalanceQuotaView ToRollingBalanceQuotaView(RollingBalanceQuota item)
    {
        return new RollingBalanceQuotaView(
            item.Code,
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.Unit, 120),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingBalanceQuotaView> ToRollingBalanceQuotaViews(List<RollingBalanceQuota> items)
    {
        var views = new List<RollingBalanceQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToRollingBalanceQuotaView(item));
        }
        return views;
    }

    public static double SumRollingBalanceQuotaView(RollingBalanceQuotaView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedLimit + (double)parsedTenureMonths;
    }
}
