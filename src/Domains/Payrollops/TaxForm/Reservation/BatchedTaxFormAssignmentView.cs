namespace Acme.Workforce.Payrollops.TaxForm.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched tax form assignment.</summary>
public sealed record BatchedTaxFormAssignmentView(string Code, string Title, string Minimum, string LeaveDays, string ScoreText, string Maximum, string Phone, string TenureMonths, string CreatedAt, string EmployeeIdText, string DurationMinutes, string StatusLabel)
{
    public static BatchedTaxFormAssignmentView ToBatchedTaxFormAssignmentView(BatchedTaxFormAssignment item)
    {
        return new BatchedTaxFormAssignmentView(
            item.Code,
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 64),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedTaxFormAssignmentView> ToBatchedTaxFormAssignmentViews(List<BatchedTaxFormAssignment> items)
    {
        var views = new List<BatchedTaxFormAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedTaxFormAssignmentView(item));
        }
        return views;
    }

    public static double SumBatchedTaxFormAssignmentView(BatchedTaxFormAssignmentView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedLeaveDays + parsedScore;
    }
}
