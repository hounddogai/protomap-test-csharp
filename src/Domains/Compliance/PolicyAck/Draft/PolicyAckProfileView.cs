namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack profile.</summary>
public sealed record PolicyAckProfileView(string Code, string Title, string Ceiling, string Description, string Pinned, string StatusLabel)
{
    public static PolicyAckProfileView ToPolicyAckProfileView(PolicyAckProfile item)
    {
        return new PolicyAckProfileView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 120),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckProfileView> ToPolicyAckProfileViews(List<PolicyAckProfile> items)
    {
        var views = new List<PolicyAckProfileView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckProfileView(item));
        }
        return views;
    }

    public static double SumPolicyAckProfileView(PolicyAckProfileView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedCeiling;
    }
}
