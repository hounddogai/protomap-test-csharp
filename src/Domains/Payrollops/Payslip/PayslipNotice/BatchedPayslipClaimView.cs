namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched payslip claim.</summary>
public sealed record BatchedPayslipClaimView(string CodeText, string Name, string CapacityText, string ExternalRefText, string SegmentText, string StatusLabel)
{
    public static BatchedPayslipClaimView ToBatchedPayslipClaimView(BatchedPayslipClaim item)
    {
        return new BatchedPayslipClaimView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 40),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedPayslipClaimView> ToBatchedPayslipClaimViews(List<BatchedPayslipClaim> items)
    {
        var views = new List<BatchedPayslipClaimView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedPayslipClaimView(item));
        }
        return views;
    }

    public static double SumBatchedPayslipClaimView(BatchedPayslipClaimView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity;
    }
}
