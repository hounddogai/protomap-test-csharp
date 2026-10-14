namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay quota.</summary>
public sealed record RetroPayQuotaView(string Code, string NameText, string TenureMonthsText, string Title, string CreatedAt, string Limit, string Hours, string Version, string CurrencyText, string StatusLabel)
{
    public static RetroPayQuotaView ToRetroPayQuotaView(RetroPayQuota item)
    {
        return new RetroPayQuotaView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayQuotaView> ToRetroPayQuotaViews(List<RetroPayQuota> items)
    {
        var views = new List<RetroPayQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayQuotaView(item));
        }
        return views;
    }

    public static double SumRetroPayQuotaView(RetroPayQuotaView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedCreatedAt + (double)parsedLimit;
    }
}
