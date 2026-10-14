namespace Acme.Workforce.Compliance.PolicyAck.Notice;

using System.Globalization;

/// <summary>How screens and exports show a policy ack rollup.</summary>
public sealed record PolicyAckRollupView(string CodeText, string Name, string TenureMonths, string LeaveDays, string ThresholdText, string FloorValue, string StatusLabel)
{
    public static PolicyAckRollupView ToPolicyAckRollupView(PolicyAckRollup item)
    {
        return new PolicyAckRollupView(
            item.Code,
            item.Name,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckRollupView> ToPolicyAckRollupViews(List<PolicyAckRollup> items)
    {
        var views = new List<PolicyAckRollupView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckRollupView(item));
        }
        return views;
    }

    public static double SumPolicyAckRollupView(PolicyAckRollupView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedLeaveDays + parsedThreshold;
    }
}
