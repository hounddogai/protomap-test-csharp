namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default payslip threshold.</summary>
public sealed record DefaultPayslipThresholdView(string Code, string TitleText, string CountryText, string Unit, string Checksum, string Amount, string ExternalRef, string StatusLabel)
{
    public static DefaultPayslipThresholdView ToDefaultPayslipThresholdView(DefaultPayslipThreshold item)
    {
        return new DefaultPayslipThresholdView(
            item.Code,
            item.Title,
            item.Country,
            item.Unit,
            Text.Truncate(item.Checksum, 40),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPayslipThresholdView> ToDefaultPayslipThresholdViews(List<DefaultPayslipThreshold> items)
    {
        var views = new List<DefaultPayslipThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPayslipThresholdView(item));
        }
        return views;
    }

    public static double SumDefaultPayslipThresholdView(DefaultPayslipThresholdView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
