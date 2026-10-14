namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Functions of lenient policy ack template from assignment.</summary>
public static class LenientPolicyAckTemplateFromAssignment
{
    public static Dictionary<string, double> SummarizeFallbackExceptionAssignmentsForTemplate(List<FallbackExceptionAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<LenientPolicyAckTemplate> DeriveLenientPolicyAckTemplatesFromAssignment(List<FallbackExceptionAssignment> items)
    {
        var outValue = new List<LenientPolicyAckTemplate>();
        foreach (var item in items)
        {
            var derived = LenientPolicyAckTemplate.NewLenientPolicyAckTemplate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeLenientPolicyAckTemplate(LenientPolicyAckTemplate item)
    {
        return BatchedExceptionRecordPolicy.BatchedExceptionRecordWeight(6, true);
    }
}
