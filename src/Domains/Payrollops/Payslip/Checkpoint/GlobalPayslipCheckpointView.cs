namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global payslip checkpoint.</summary>
public sealed record GlobalPayslipCheckpointView(string Code, string Name, string WindowDays, string Minimum, string Country, string Amount, string Label, string Region, string PinnedText, string StatusLabel)
{
    public static GlobalPayslipCheckpointView ToGlobalPayslipCheckpointView(GlobalPayslipCheckpoint item)
    {
        return new GlobalPayslipCheckpointView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 80),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 40),
            item.Region,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPayslipCheckpointView> ToGlobalPayslipCheckpointViews(List<GlobalPayslipCheckpoint> items)
    {
        var views = new List<GlobalPayslipCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPayslipCheckpointView(item));
        }
        return views;
    }

    public static double SumGlobalPayslipCheckpointView(GlobalPayslipCheckpointView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedMinimum + parsedAmount;
    }
}
