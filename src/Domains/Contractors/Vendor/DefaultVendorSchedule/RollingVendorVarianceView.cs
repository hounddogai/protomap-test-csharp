namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling vendor variance.</summary>
public sealed record RollingVendorVarianceView(string CodeText, string NameText, string Retries, string Description, string Timezone, string Channel, string StatusLabel)
{
    public static RollingVendorVarianceView ToRollingVendorVarianceView(RollingVendorVariance item)
    {
        return new RollingVendorVarianceView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Timezone,
            Text.Truncate(item.Channel, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingVendorVarianceView> ToRollingVendorVarianceViews(List<RollingVendorVariance> items)
    {
        var views = new List<RollingVendorVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToRollingVendorVarianceView(item));
        }
        return views;
    }

    public static double SumRollingVendorVarianceView(RollingVendorVarianceView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
