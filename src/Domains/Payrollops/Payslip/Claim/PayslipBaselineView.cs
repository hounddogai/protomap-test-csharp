namespace Acme.Workforce.Payrollops.Payslip.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip baseline.</summary>
public sealed record PayslipBaselineView(string Code, string Title, string HoursText, string Sequence, string ExternalRefText, string UpdatedAtText, string Locale, string Ratio, string QuantityText, string StatusLabel)
{
    public static PayslipBaselineView ToPayslipBaselineView(PayslipBaseline item)
    {
        return new PayslipBaselineView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 40),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 80),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipBaselineView> ToPayslipBaselineViews(List<PayslipBaseline> items)
    {
        var views = new List<PayslipBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipBaselineView(item));
        }
        return views;
    }

    public static double SumPayslipBaselineView(PayslipBaselineView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedSequence + (double)parsedUpdatedAt;
    }
}
