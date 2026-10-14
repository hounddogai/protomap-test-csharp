namespace Acme.Workforce.Payrollops.Payslip.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip group.</summary>
public sealed record PayslipGroupView(string Code, string Label, string TenureMonths, string ChecksumText, string FloorValue, string StartsAt, string ContactEmailText, string StatusLabel)
{
    public static PayslipGroupView ToPayslipGroupView(PayslipGroup item)
    {
        return new PayslipGroupView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipGroupView> ToPayslipGroupViews(List<PayslipGroup> items)
    {
        var views = new List<PayslipGroupView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipGroupView(item));
        }
        return views;
    }

    public static double SumPayslipGroupView(PayslipGroupView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedFloorValue + (double)parsedStartsAt;
    }
}
