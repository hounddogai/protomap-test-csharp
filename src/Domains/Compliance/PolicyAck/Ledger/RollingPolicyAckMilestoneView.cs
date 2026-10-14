namespace Acme.Workforce.Compliance.PolicyAck.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling policy ack milestone.</summary>
public sealed record RollingPolicyAckMilestoneView(string CodeText, string Title, string Weight, string RatioText, string LeaveDays, string GraceDays, string ExternalRefText, string EmployeeId, string StatusLabel)
{
    public static RollingPolicyAckMilestoneView ToRollingPolicyAckMilestoneView(RollingPolicyAckMilestone item)
    {
        return new RollingPolicyAckMilestoneView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPolicyAckMilestoneView> ToRollingPolicyAckMilestoneViews(List<RollingPolicyAckMilestone> items)
    {
        var views = new List<RollingPolicyAckMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPolicyAckMilestoneView(item));
        }
        return views;
    }

    public static double SumRollingPolicyAckMilestoneView(RollingPolicyAckMilestoneView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedWeight + parsedRatio + (double)parsedLeaveDays;
    }
}
