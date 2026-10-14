namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped payslip batch.</summary>
public sealed record CappedPayslipBatchView(string CodeText, string Label, string ExternalRef, string UpdatedAt, string StartsAt, string Discount, string ContactEmail, string LocaleText, string StatusLabel)
{
    public static CappedPayslipBatchView ToCappedPayslipBatchView(CappedPayslipBatch item)
    {
        return new CappedPayslipBatchView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.ExternalRef,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            Text.Truncate(item.Locale, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPayslipBatchView> ToCappedPayslipBatchViews(List<CappedPayslipBatch> items)
    {
        var views = new List<CappedPayslipBatchView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPayslipBatchView(item));
        }
        return views;
    }

    public static double SumCappedPayslipBatchView(CappedPayslipBatchView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedStartsAt + parsedDiscount;
    }
}
