namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip threshold.</summary>
public sealed record PayslipThresholdView(string Code, string Label, string GraceDays, string ContactEmail, string RegionText, string Checksum, string StatusLabel)
{
    public static PayslipThresholdView ToPayslipThresholdView(PayslipThreshold item)
    {
        return new PayslipThresholdView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            Text.Truncate(item.Region, 120),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipThresholdView> ToPayslipThresholdViews(List<PayslipThreshold> items)
    {
        var views = new List<PayslipThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipThresholdView(item));
        }
        return views;
    }

    public static double SumPayslipThresholdView(PayslipThresholdView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays;
    }
}
