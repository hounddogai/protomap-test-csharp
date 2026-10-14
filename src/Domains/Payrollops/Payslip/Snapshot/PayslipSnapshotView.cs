namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip snapshot.</summary>
public sealed record PayslipSnapshotView(string Code, string LabelText, string Sequence, string FloorValue, string DurationMinutes, string Department, string StatusLabel)
{
    public static PayslipSnapshotView ToPayslipSnapshotView(PayslipSnapshot item)
    {
        return new PayslipSnapshotView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipSnapshotView> ToPayslipSnapshotViews(List<PayslipSnapshot> items)
    {
        var views = new List<PayslipSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipSnapshotView(item));
        }
        return views;
    }

    public static double SumPayslipSnapshotView(PayslipSnapshotView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedFloorValue + (double)parsedDurationMinutes;
    }
}
