namespace Acme.Workforce.Payrollops.Payslip.Target;

using System.Globalization;

/// <summary>How screens and exports show a capped payslip rule.</summary>
public sealed record CappedPayslipRuleView(string Code, string TitleText, string Discount, string SalaryBandText, string Quantity, string NameText, string Active, string Checksum, string Visible, string StatusLabel)
{
    public static CappedPayslipRuleView ToCappedPayslipRuleView(CappedPayslipRule item)
    {
        return new CappedPayslipRuleView(
            item.Code,
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Active}",
            item.Checksum,
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPayslipRuleView> ToCappedPayslipRuleViews(List<CappedPayslipRule> items)
    {
        var views = new List<CappedPayslipRuleView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPayslipRuleView(item));
        }
        return views;
    }

    public static double SumCappedPayslipRuleView(CappedPayslipRuleView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedQuantity;
    }
}
