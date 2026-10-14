namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack override.</summary>
public sealed record PolicyAckOverrideView(string Code, string LabelText, string Reviewer, string Notes, string CeilingText, string Channel, string Skill, string StatusLabel)
{
    public static PolicyAckOverrideView ToPolicyAckOverrideView(PolicyAckOverride item)
    {
        return new PolicyAckOverrideView(
            item.Code,
            item.Label,
            item.Reviewer,
            Text.Truncate(item.Notes, 64),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckOverrideView> ToPolicyAckOverrideViews(List<PolicyAckOverride> items)
    {
        var views = new List<PolicyAckOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckOverrideView(item));
        }
        return views;
    }

    public static double SumPolicyAckOverrideView(PolicyAckOverrideView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedCeiling;
    }
}
