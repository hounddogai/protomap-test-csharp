namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack allocation.</summary>
public sealed record PolicyAckAllocationView(string CodeText, string Label, string VersionText, string GradeText, string LimitText, string OwnerId, string TenureMonthsText, string Cost, string Location, string Pinned, string StatusLabel)
{
    public static PolicyAckAllocationView ToPolicyAckAllocationView(PolicyAckAllocation item)
    {
        return new PolicyAckAllocationView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckAllocationView> ToPolicyAckAllocationViews(List<PolicyAckAllocation> items)
    {
        var views = new List<PolicyAckAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckAllocationView(item));
        }
        return views;
    }

    public static double SumPolicyAckAllocationView(PolicyAckAllocationView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedLimit + (double)parsedTenureMonths;
    }
}
