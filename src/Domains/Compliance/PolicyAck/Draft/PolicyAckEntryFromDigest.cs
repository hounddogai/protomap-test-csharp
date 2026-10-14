namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Functions of policy ack entry from digest.</summary>
public static class PolicyAckEntryFromDigest
{
    public static Dictionary<string, double> SummarizeDefaultExceptionDigestsForEntry(List<DefaultExceptionDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<PolicyAckEntry> DerivePolicyAckEntriesFromDigest(List<DefaultExceptionDigest> items)
    {
        var outValue = new List<PolicyAckEntry>();
        foreach (var item in items)
        {
            var derived = PolicyAckEntry.NewPolicyAckEntry(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizePolicyAckEntry(PolicyAckEntry item)
    {
        return ExceptionRequestPolicy.NormalizeExceptionRequestCode(item.Code);
    }
}
