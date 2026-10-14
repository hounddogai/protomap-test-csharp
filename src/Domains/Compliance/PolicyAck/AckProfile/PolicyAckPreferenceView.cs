namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack preference.</summary>
public sealed record PolicyAckPreferenceView(string Code, string Name, string EmployeeId, string MarginText, string FloorValue, string LimitText, string ManagerId, string StatusLabel)
{
    public static PolicyAckPreferenceView ToPolicyAckPreferenceView(PolicyAckPreference item)
    {
        return new PolicyAckPreferenceView(
            item.Code,
            item.Name,
            Text.Truncate(item.EmployeeId, 120),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckPreferenceView> ToPolicyAckPreferenceViews(List<PolicyAckPreference> items)
    {
        var views = new List<PolicyAckPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckPreferenceView(item));
        }
        return views;
    }

    public static double SumPolicyAckPreferenceView(PolicyAckPreferenceView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return parsedMargin + parsedFloorValue + (double)parsedLimit;
    }
}
