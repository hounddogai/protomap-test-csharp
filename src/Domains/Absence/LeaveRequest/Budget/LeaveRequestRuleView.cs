namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request rule.</summary>
public sealed record LeaveRequestRuleView(string CodeText, string LabelText, string CapacityText, string Description, string Maximum, string RetriesText, string Score, string Amount, string Country, string StatusLabel)
{
    public static LeaveRequestRuleView ToLeaveRequestRuleView(LeaveRequestRule item)
    {
        return new LeaveRequestRuleView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestRuleView> ToLeaveRequestRuleViews(List<LeaveRequestRule> items)
    {
        var views = new List<LeaveRequestRuleView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestRuleView(item));
        }
        return views;
    }

    public static double SumLeaveRequestRuleView(LeaveRequestRuleView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedMaximum + (double)parsedRetries;
    }
}
