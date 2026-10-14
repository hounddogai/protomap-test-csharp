namespace Acme.Workforce.Compliance.PolicyAck.Notice;

using Acme.Workforce.Compliance.Exception.Baseline;

/// <summary>Functions of policy ack notice from baseline.</summary>
public static class PolicyAckNoticeFromBaseline
{
    public static Dictionary<string, double> SummarizeExceptionBaselinesForNotice(List<ExceptionBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<PolicyAckNotice> DerivePolicyAckNoticesFromBaseline(List<ExceptionBaseline> items)
    {
        var outValue = new List<PolicyAckNotice>();
        foreach (var item in items)
        {
            var derived = PolicyAckNotice.NewPolicyAckNotice(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankPolicyAckNotice(PolicyAckNotice item)
    {
        return ExceptionBundlePolicy.ExceptionBundleBucket(item.MeasuredValue());
    }
}
