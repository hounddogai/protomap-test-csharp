namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Compliance.TrainingDue.OverrideItem;

/// <summary>Functions of default policy ack exception from audit.</summary>
public static class DefaultPolicyAckExceptionFromAudit
{
    public static Dictionary<string, double> SummarizeTieredTrainingDueAuditsForException(List<TieredTrainingDueAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<DefaultPolicyAckException> DeriveDefaultPolicyAckExceptionsFromAudit(List<TieredTrainingDueAudit> items)
    {
        var outValue = new List<DefaultPolicyAckException>();
        foreach (var item in items)
        {
            var derived = DefaultPolicyAckException.NewDefaultPolicyAckException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitDefaultPolicyAckException(DefaultPolicyAckException item)
    {
        return TrainingDueOverridePolicy.ClassifyTrainingDueOverride(item.NetVolume());
    }
}
