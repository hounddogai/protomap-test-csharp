namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

using Acme.Workforce.Talent.Nomination.Rollup;

/// <summary>Functions of primary mobility audit from exception.</summary>
public static class PrimaryMobilityAuditFromException
{
    public static Dictionary<string, double> SummarizeFallbackNominationExceptionsForAudit(List<FallbackNominationException> items)
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

    public static List<PrimaryMobilityAudit> DerivePrimaryMobilityAuditsFromException(List<FallbackNominationException> items)
    {
        var outValue = new List<PrimaryMobilityAudit>();
        foreach (var item in items)
        {
            var derived = PrimaryMobilityAudit.NewPrimaryMobilityAudit(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankPrimaryMobilityAudit(PrimaryMobilityAudit item)
    {
        return NominationEntryPolicy.ClassifyNominationEntry(item.MeasuredValue());
    }
}
