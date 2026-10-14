namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered leave request plan.</summary>
public sealed record TieredLeaveRequestPlanView(string CodeText, string Label, string WeightText, string Skill, string ReviewerText, string Attempts, string Amount, string StatusLabel)
{
    public static TieredLeaveRequestPlanView ToTieredLeaveRequestPlanView(TieredLeaveRequestPlan item)
    {
        return new TieredLeaveRequestPlanView(
            item.Code,
            item.Label,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 80),
            item.Reviewer,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredLeaveRequestPlanView> ToTieredLeaveRequestPlanViews(List<TieredLeaveRequestPlan> items)
    {
        var views = new List<TieredLeaveRequestPlanView>();
        foreach (var item in items)
        {
            views.Add(ToTieredLeaveRequestPlanView(item));
        }
        return views;
    }

    public static double SumTieredLeaveRequestPlanView(TieredLeaveRequestPlanView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedAttempts + parsedAmount;
    }
}
