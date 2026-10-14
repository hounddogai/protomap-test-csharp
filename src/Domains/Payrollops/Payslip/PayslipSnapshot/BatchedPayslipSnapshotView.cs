namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched payslip snapshot.</summary>
public sealed record BatchedPayslipSnapshotView(string CodeText, string Name, string SalaryBand, string Hours, string CreatedAt, string RegionText, string StatusLabel)
{
    public static BatchedPayslipSnapshotView ToBatchedPayslipSnapshotView(BatchedPayslipSnapshot item)
    {
        return new BatchedPayslipSnapshotView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.SalaryBand,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedPayslipSnapshotView> ToBatchedPayslipSnapshotViews(List<BatchedPayslipSnapshot> items)
    {
        var views = new List<BatchedPayslipSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedPayslipSnapshotView(item));
        }
        return views;
    }

    public static double SumBatchedPayslipSnapshotView(BatchedPayslipSnapshotView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedCreatedAt;
    }
}
