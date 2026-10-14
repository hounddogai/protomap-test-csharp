namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Functions of capped attestation proposal from metric.</summary>
public static class CappedAttestationProposalFromMetric
{
    public static Dictionary<string, double> SummarizeSeasonalExceptionMetricsForProposal(List<SeasonalExceptionMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<CappedAttestationProposal> DeriveCappedAttestationProposalsFromMetric(List<SeasonalExceptionMetric> items)
    {
        var outValue = new List<CappedAttestationProposal>();
        foreach (var item in items)
        {
            var derived = CappedAttestationProposal.NewCappedAttestationProposal(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitCappedAttestationProposal(CappedAttestationProposal item)
    {
        return ExceptionTemplatePolicy.NormalizeExceptionTemplateCode(item.Code);
    }
}
