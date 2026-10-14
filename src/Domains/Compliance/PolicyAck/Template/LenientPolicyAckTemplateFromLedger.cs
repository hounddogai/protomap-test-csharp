namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Compliance.Audit.Cycle;

/// <summary>Functions of lenient policy ack template from ledger.</summary>
public static class LenientPolicyAckTemplateFromLedger
{
    public static Dictionary<string, double> SummarizeWeightedAuditLedgersForTemplate(List<WeightedAuditLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<LenientPolicyAckTemplate> DeriveLenientPolicyAckTemplatesFromLedger(List<WeightedAuditLedger> items)
    {
        var outValue = new List<LenientPolicyAckTemplate>();
        foreach (var item in items)
        {
            var derived = LenientPolicyAckTemplate.NewLenientPolicyAckTemplate(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankLenientPolicyAckTemplate(LenientPolicyAckTemplate item)
    {
        return AuditCyclePolicy.AuditCycleBucket(item.ProjectedValue());
    }
}
