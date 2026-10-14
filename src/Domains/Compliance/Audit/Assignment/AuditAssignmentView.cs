namespace Acme.Workforce.Compliance.Audit.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit assignment.</summary>
public sealed record AuditAssignmentView(string Code, string Name, string Priority, string JobCode, string Location, string QuantityText, string Approved, string StatusLabel)
{
    public static AuditAssignmentView ToAuditAssignmentView(AuditAssignment item)
    {
        return new AuditAssignmentView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 40),
            item.Location,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditAssignmentView> ToAuditAssignmentViews(List<AuditAssignment> items)
    {
        var views = new List<AuditAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToAuditAssignmentView(item));
        }
        return views;
    }

    public static double SumAuditAssignmentView(AuditAssignmentView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedQuantity;
    }
}
