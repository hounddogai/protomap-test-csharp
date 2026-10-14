namespace Acme.Workforce.Absence.Policy.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy assignment.</summary>
public sealed record PolicyAssignmentView(string Code, string TitleText, string Maximum, string Quantity, string Badge, string SegmentText, string LeaveDays, string StatusLabel)
{
    public static PolicyAssignmentView ToPolicyAssignmentView(PolicyAssignment item)
    {
        return new PolicyAssignmentView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            Text.Truncate(item.Segment, 40),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAssignmentView> ToPolicyAssignmentViews(List<PolicyAssignment> items)
    {
        var views = new List<PolicyAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAssignmentView(item));
        }
        return views;
    }

    public static double SumPolicyAssignmentView(PolicyAssignmentView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedQuantity + (double)parsedLeaveDays;
    }
}
