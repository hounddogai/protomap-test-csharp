namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback payslip checkpoint.</summary>
public sealed record FallbackPayslipCheckpointView(string Code, string NameText, string Attempts, string Amount, string Currency, string Department, string Summary, string BatchSize, string StatusLabel)
{
    public static FallbackPayslipCheckpointView ToFallbackPayslipCheckpointView(FallbackPayslipCheckpoint item)
    {
        return new FallbackPayslipCheckpointView(
            item.Code,
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Department,
            Text.Truncate(item.Summary, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPayslipCheckpointView> ToFallbackPayslipCheckpointViews(List<FallbackPayslipCheckpoint> items)
    {
        var views = new List<FallbackPayslipCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPayslipCheckpointView(item));
        }
        return views;
    }

    public static double SumFallbackPayslipCheckpointView(FallbackPayslipCheckpointView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedAmount + (double)parsedBatchSize;
    }
}
