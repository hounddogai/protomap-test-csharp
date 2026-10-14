namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip rollup.</summary>
public sealed record PayslipRollupView(string Code, string NameText, string RetriesText, string Maximum, string DiscountText, string TenureMonths, string Description, string Unit, string ChannelText, string StatusLabel)
{
    public static PayslipRollupView ToPayslipRollupView(PayslipRollup item)
    {
        return new PayslipRollupView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 80),
            item.Unit,
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipRollupView> ToPayslipRollupViews(List<PayslipRollup> items)
    {
        var views = new List<PayslipRollupView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipRollupView(item));
        }
        return views;
    }

    public static double SumPayslipRollupView(PayslipRollupView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedMaximum + parsedDiscount;
    }
}
