namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary leave request budget.</summary>
public sealed record PrimaryLeaveRequestBudgetView(string CodeText, string TitleText, string CreatedAtText, string BatchSize, string QuantityText, string Threshold, string Weight, string Name, string ExternalRef, string Maximum, string StatusLabel)
{
    public static PrimaryLeaveRequestBudgetView ToPrimaryLeaveRequestBudgetView(PrimaryLeaveRequestBudget item)
    {
        return new PrimaryLeaveRequestBudgetView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.ExternalRef,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryLeaveRequestBudgetView> ToPrimaryLeaveRequestBudgetViews(List<PrimaryLeaveRequestBudget> items)
    {
        var views = new List<PrimaryLeaveRequestBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryLeaveRequestBudgetView(item));
        }
        return views;
    }

    public static double SumPrimaryLeaveRequestBudgetView(PrimaryLeaveRequestBudgetView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedBatchSize + (double)parsedQuantity;
    }
}
