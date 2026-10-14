namespace Acme.Workforce.Compliance.Attestation.Schedule;

using Acme.Workforce.Compliance.Exception.Proposal;

/// <summary>Functions of attestation schedule from override.</summary>
public static class AttestationScheduleFromOverride
{
    public static Dictionary<string, double> SummarizeExceptionOverridesForSchedule(List<ExceptionOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<AttestationSchedule> DeriveAttestationSchedulesFromOverride(List<ExceptionOverride> items)
    {
        var outValue = new List<AttestationSchedule>();
        foreach (var item in items)
        {
            var derived = AttestationSchedule.NewAttestationSchedule(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchAttestationSchedule(AttestationSchedule item)
    {
        return ExceptionProposalPolicy.NormalizeExceptionProposalCode(item.Code);
    }
}
