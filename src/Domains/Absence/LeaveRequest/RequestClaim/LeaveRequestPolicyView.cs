namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request policy.</summary>
public sealed record LeaveRequestPolicyView(string Code, string Label, string Ratio, string Budget, string NotesText, string FloorValueText, string StatusLabel)
{
    public static LeaveRequestPolicyView ToLeaveRequestPolicyView(LeaveRequestPolicy item)
    {
        return new LeaveRequestPolicyView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestPolicyView> ToLeaveRequestPolicyViews(List<LeaveRequestPolicy> items)
    {
        var views = new List<LeaveRequestPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestPolicyView(item));
        }
        return views;
    }

    public static double SumLeaveRequestPolicyView(LeaveRequestPolicyView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return parsedRatio + parsedBudget + parsedFloorValue;
    }
}
