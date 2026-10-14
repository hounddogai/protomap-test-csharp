namespace Acme.Workforce.Compliance.PolicyAck.Schedule;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Functions of tiered policy ack schedule from template.</summary>
public static class TieredPolicyAckScheduleFromTemplate
{
    public static Dictionary<string, double> SummarizeLegacyAttestationTemplatesForSchedule(List<LegacyAttestationTemplate> items)
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

    public static List<TieredPolicyAckSchedule> DeriveTieredPolicyAckSchedulesFromTemplate(List<LegacyAttestationTemplate> items)
    {
        var outValue = new List<TieredPolicyAckSchedule>();
        foreach (var item in items)
        {
            var derived = TieredPolicyAckSchedule.NewTieredPolicyAckSchedule(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyTieredPolicyAckSchedule(TieredPolicyAckSchedule item)
    {
        return PrimaryAttestationPolicyPolicy.ClassifyPrimaryAttestationPolicy(item.ProjectedTotal());
    }
}
