namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled training due audit.</summary>
public sealed record ScheduledTrainingDueAuditView(string Code, string Name, string SalaryBand, string VisibleText, string EmployeeId, string QuantityText, string StatusLabel)
{
    public static ScheduledTrainingDueAuditView ToScheduledTrainingDueAuditView(ScheduledTrainingDueAudit item)
    {
        return new ScheduledTrainingDueAuditView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.SalaryBand,
            $"{item.Visible}",
            Text.Truncate(item.EmployeeId, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledTrainingDueAuditView> ToScheduledTrainingDueAuditViews(List<ScheduledTrainingDueAudit> items)
    {
        var views = new List<ScheduledTrainingDueAuditView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledTrainingDueAuditView(item));
        }
        return views;
    }

    public static double SumScheduledTrainingDueAuditView(ScheduledTrainingDueAuditView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
