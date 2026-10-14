namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling cost model audit.</summary>
public sealed record RollingCostModelAuditView(string Code, string Name, string Maximum, string UpdatedAt, string QuantityText, string CapacityText, string VersionText, string AmountText, string BudgetText, string BatchSize, string Attempts, string StatusLabel)
{
    public static RollingCostModelAuditView ToRollingCostModelAuditView(RollingCostModelAudit item)
    {
        return new RollingCostModelAuditView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingCostModelAuditView> ToRollingCostModelAuditViews(List<RollingCostModelAudit> items)
    {
        var views = new List<RollingCostModelAuditView>();
        foreach (var item in items)
        {
            views.Add(ToRollingCostModelAuditView(item));
        }
        return views;
    }

    public static double SumRollingCostModelAuditView(RollingCostModelAuditView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedUpdatedAt + (double)parsedQuantity;
    }
}
