namespace Acme.Workforce.Payrollops.Payslip.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip slot.</summary>
public sealed record PayslipSlotView(string Code, string Title, string Ceiling, string Department, string MarginText, string Maximum, string ContactEmail, string Source, string RateText, string Badge, string Sequence, string StatusLabel)
{
    public static PayslipSlotView ToPayslipSlotView(PayslipSlot item)
    {
        return new PayslipSlotView(
            item.Code,
            item.Title,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 64),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Source,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipSlotView> ToPayslipSlotViews(List<PayslipSlot> items)
    {
        var views = new List<PayslipSlotView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipSlotView(item));
        }
        return views;
    }

    public static double SumPayslipSlotView(PayslipSlotView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedMargin + parsedMaximum;
    }
}
