namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched leave request snapshot.</summary>
public sealed record BatchedLeaveRequestSnapshotView(string CodeText, string Name, string Active, string SalaryBand, string OwnerId, string Quantity, string StatusLabel)
{
    public static BatchedLeaveRequestSnapshotView ToBatchedLeaveRequestSnapshotView(BatchedLeaveRequestSnapshot item)
    {
        return new BatchedLeaveRequestSnapshotView(
            item.Code,
            Text.Truncate(item.Name, 80),
            $"{item.Active}",
            item.SalaryBand,
            Text.Truncate(item.OwnerId, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedLeaveRequestSnapshotView> ToBatchedLeaveRequestSnapshotViews(List<BatchedLeaveRequestSnapshot> items)
    {
        var views = new List<BatchedLeaveRequestSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedLeaveRequestSnapshotView(item));
        }
        return views;
    }

    public static double SumBatchedLeaveRequestSnapshotView(BatchedLeaveRequestSnapshotView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
