namespace Acme.Workforce.Contractors.Vendor.VendorSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling vendor quota.</summary>
public sealed record RollingVendorQuotaView(string Code, string LabelText, string Hours, string Attempts, string Channel, string Discount, string WindowDays, string Threshold, string LocaleText, string Department, string Source, string StatusLabel)
{
    public static RollingVendorQuotaView ToRollingVendorQuotaView(RollingVendorQuota item)
    {
        return new RollingVendorQuotaView(
            item.Code,
            item.Label,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            Text.Truncate(item.Department, 64),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingVendorQuotaView> ToRollingVendorQuotaViews(List<RollingVendorQuota> items)
    {
        var views = new List<RollingVendorQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToRollingVendorQuotaView(item));
        }
        return views;
    }

    public static double SumRollingVendorQuotaView(RollingVendorQuotaView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedAttempts + parsedDiscount;
    }
}
