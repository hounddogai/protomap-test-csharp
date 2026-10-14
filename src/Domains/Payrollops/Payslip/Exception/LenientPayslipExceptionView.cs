namespace Acme.Workforce.Payrollops.Payslip.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient payslip exception.</summary>
public sealed record LenientPayslipExceptionView(string Code, string Label, string Name, string Discount, string Segment, string ManagerId, string FloorValueText, string FteText, string StatusLabel)
{
    public static LenientPayslipExceptionView ToLenientPayslipExceptionView(LenientPayslipException item)
    {
        return new LenientPayslipExceptionView(
            Text.Truncate(item.Code, 64),
            item.Label,
            Text.Truncate(item.Name, 120),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 64),
            Text.Truncate(item.ManagerId, 120),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientPayslipExceptionView> ToLenientPayslipExceptionViews(List<LenientPayslipException> items)
    {
        var views = new List<LenientPayslipExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToLenientPayslipExceptionView(item));
        }
        return views;
    }

    public static double SumLenientPayslipExceptionView(LenientPayslipExceptionView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedFloorValue + parsedFte;
    }
}
