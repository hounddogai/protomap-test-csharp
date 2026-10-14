namespace Acme.Workforce.Absence.LeaveRequest.Item;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request score.</summary>
public sealed record LeaveRequestScoreView(string CodeText, string Title, string EmployeeId, string Source, string CostText, string StatusLabel)
{
    public static LeaveRequestScoreView ToLeaveRequestScoreView(LeaveRequestScore item)
    {
        return new LeaveRequestScoreView(
            Text.Truncate(item.Code, 80),
            item.Title,
            Text.Truncate(item.EmployeeId, 80),
            item.Source,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestScoreView> ToLeaveRequestScoreViews(List<LeaveRequestScore> items)
    {
        var views = new List<LeaveRequestScoreView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestScoreView(item));
        }
        return views;
    }

    public static double SumLeaveRequestScoreView(LeaveRequestScoreView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
