namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary payslip snapshot.</summary>
public sealed record PrimaryPayslipSnapshotView(string CodeText, string Label, string Location, string TeamText, string DurationMinutes, string Phone, string EmployeeId, string Category, string Locale, string GraceDays, string StatusLabel)
{
    public static PrimaryPayslipSnapshotView ToPrimaryPayslipSnapshotView(PrimaryPayslipSnapshot item)
    {
        return new PrimaryPayslipSnapshotView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 120),
            item.Location,
            item.Team,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 120),
            Text.Truncate(item.EmployeeId, 64),
            item.Category,
            item.Locale,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPayslipSnapshotView> ToPrimaryPayslipSnapshotViews(List<PrimaryPayslipSnapshot> items)
    {
        var views = new List<PrimaryPayslipSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPayslipSnapshotView(item));
        }
        return views;
    }

    public static double SumPrimaryPayslipSnapshotView(PrimaryPayslipSnapshotView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedGraceDays;
    }
}
