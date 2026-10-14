namespace Acme.Workforce.Compliance.PolicyAck.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack schedule.</summary>
public sealed record PolicyAckScheduleView(string Code, string TitleText, string SegmentText, string Revision, string GraceDays, string BudgetText, string StartsAt, string LeaveDays, string StatusLabel)
{
    public static PolicyAckScheduleView ToPolicyAckScheduleView(PolicyAckSchedule item)
    {
        return new PolicyAckScheduleView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Segment,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckScheduleView> ToPolicyAckScheduleViews(List<PolicyAckSchedule> items)
    {
        var views = new List<PolicyAckScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckScheduleView(item));
        }
        return views;
    }

    public static double SumPolicyAckScheduleView(PolicyAckScheduleView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedGraceDays + parsedBudget;
    }
}
