namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal payslip snapshot.</summary>
public sealed record SeasonalPayslipSnapshotView(string Code, string Label, string UpdatedAtText, string Limit, string Timezone, string StatusLabel)
{
    public static SeasonalPayslipSnapshotView ToSeasonalPayslipSnapshotView(SeasonalPayslipSnapshot item)
    {
        return new SeasonalPayslipSnapshotView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPayslipSnapshotView> ToSeasonalPayslipSnapshotViews(List<SeasonalPayslipSnapshot> items)
    {
        var views = new List<SeasonalPayslipSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPayslipSnapshotView(item));
        }
        return views;
    }

    public static double SumSeasonalPayslipSnapshotView(SeasonalPayslipSnapshotView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedLimit;
    }
}
