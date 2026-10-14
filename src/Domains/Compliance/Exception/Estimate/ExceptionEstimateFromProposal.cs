namespace Acme.Workforce.Compliance.Exception.Estimate;

using Acme.Workforce.Compliance.Certification.Assignment;

/// <summary>Functions of exception estimate from proposal.</summary>
public static class ExceptionEstimateFromProposal
{
    public static Dictionary<string, double> SummarizeAdaptiveCertificationProposalsForEstimate(List<AdaptiveCertificationProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<ExceptionEstimate> DeriveExceptionEstimatesFromProposal(List<AdaptiveCertificationProposal> items)
    {
        var outValue = new List<ExceptionEstimate>();
        foreach (var item in items)
        {
            var derived = ExceptionEstimate.NewExceptionEstimate(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleExceptionEstimate(ExceptionEstimate item)
    {
        return CertificationAssignmentPolicy.NormalizeCertificationAssignmentCode(item.Code);
    }
}
