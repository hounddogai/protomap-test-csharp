namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using System.Globalization;

/// <summary>How screens and exports show a legacy policy ack group.</summary>
public sealed record LegacyPolicyAckGroupView(string CodeText, string LabelText, string ChannelText, string CategoryText, string LimitText, string Country, string UpdatedAtText, string Skill, string PriorityText, string Phone, string StatusLabel)
{
    public static LegacyPolicyAckGroupView ToLegacyPolicyAckGroupView(LegacyPolicyAckGroup item)
    {
        return new LegacyPolicyAckGroupView(
            item.Code,
            item.Label,
            item.Channel,
            item.Category,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPolicyAckGroupView> ToLegacyPolicyAckGroupViews(List<LegacyPolicyAckGroup> items)
    {
        var views = new List<LegacyPolicyAckGroupView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPolicyAckGroupView(item));
        }
        return views;
    }

    public static double SumLegacyPolicyAckGroupView(LegacyPolicyAckGroupView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedUpdatedAt + (double)parsedPriority;
    }
}
