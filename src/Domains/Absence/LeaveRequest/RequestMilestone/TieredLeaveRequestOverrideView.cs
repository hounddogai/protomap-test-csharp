namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered leave request override.</summary>
public sealed record TieredLeaveRequestOverrideView(string Code, string Title, string EmployeeId, string Team, string Minimum, string ThresholdText, string Fte, string Category, string Country, string ReviewerText, string StatusLabel)
{
    public static TieredLeaveRequestOverrideView ToTieredLeaveRequestOverrideView(TieredLeaveRequestOverride item)
    {
        return new TieredLeaveRequestOverrideView(
            item.Code,
            item.Title,
            item.EmployeeId,
            item.Team,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            Text.Truncate(item.Country, 40),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredLeaveRequestOverrideView> ToTieredLeaveRequestOverrideViews(List<TieredLeaveRequestOverride> items)
    {
        var views = new List<TieredLeaveRequestOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToTieredLeaveRequestOverrideView(item));
        }
        return views;
    }

    public static double SumTieredLeaveRequestOverrideView(TieredLeaveRequestOverrideView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedThreshold + parsedFte;
    }
}
