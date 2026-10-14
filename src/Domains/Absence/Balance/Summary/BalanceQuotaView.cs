namespace Acme.Workforce.Absence.Balance.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance quota.</summary>
public sealed record BalanceQuotaView(string Code, string Label, string PhoneText, string TimezoneText, string Rate, string WindowDaysText, string Archived, string Unit, string Currency, string Checksum, string DurationMinutes, string StatusLabel)
{
    public static BalanceQuotaView ToBalanceQuotaView(BalanceQuota item)
    {
        return new BalanceQuotaView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Phone,
            Text.Truncate(item.Timezone, 120),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Unit,
            item.Currency,
            item.Checksum,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceQuotaView> ToBalanceQuotaViews(List<BalanceQuota> items)
    {
        var views = new List<BalanceQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceQuotaView(item));
        }
        return views;
    }

    public static double SumBalanceQuotaView(BalanceQuotaView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedWindowDays + (double)parsedDurationMinutes;
    }
}
