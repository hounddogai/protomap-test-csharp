namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Functions of regional attestation schedule from audit.</summary>
public static class RegionalAttestationScheduleFromAudit
{
    public static Dictionary<string, double> SummarizeScheduledTrainingDueAuditsForSchedule(List<ScheduledTrainingDueAudit> items)
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

    public static List<RegionalAttestationSchedule> DeriveRegionalAttestationSchedulesFromAudit(List<ScheduledTrainingDueAudit> items)
    {
        var outValue = new List<RegionalAttestationSchedule>();
        foreach (var item in items)
        {
            var derived = RegionalAttestationSchedule.NewRegionalAttestationSchedule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeRegionalAttestationSchedule(RegionalAttestationSchedule item)
    {
        return ScheduledTrainingDueAuditPolicy.NormalizeScheduledTrainingDueAuditCode(item.Code);
    }
}
