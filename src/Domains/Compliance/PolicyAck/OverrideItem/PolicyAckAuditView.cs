namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack audit.</summary>
public sealed record PolicyAckAuditView(string Code, string Label, string Discount, string EmployeeId, string Summary, string WindowDaysText, string Category, string FloorValue, string Threshold, string StatusLabel)
{
    public static PolicyAckAuditView ToPolicyAckAuditView(PolicyAckAudit item)
    {
        return new PolicyAckAuditView(
            item.Code,
            item.Label,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            Text.Truncate(item.Summary, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckAuditView> ToPolicyAckAuditViews(List<PolicyAckAudit> items)
    {
        var views = new List<PolicyAckAuditView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckAuditView(item));
        }
        return views;
    }

    public static double SumPolicyAckAuditView(PolicyAckAuditView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedWindowDays + parsedFloorValue;
    }
}
