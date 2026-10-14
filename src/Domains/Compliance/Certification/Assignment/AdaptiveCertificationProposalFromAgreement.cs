namespace Acme.Workforce.Compliance.Certification.Assignment;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Functions of adaptive certification proposal from agreement.</summary>
public static class AdaptiveCertificationProposalFromAgreement
{
    public static Dictionary<string, double> SummarizeAttestationAgreementsForProposal(List<AttestationAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<AdaptiveCertificationProposal> DeriveAdaptiveCertificationProposalsFromAgreement(List<AttestationAgreement> items)
    {
        var outValue = new List<AdaptiveCertificationProposal>();
        foreach (var item in items)
        {
            var derived = AdaptiveCertificationProposal.NewAdaptiveCertificationProposal(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignAdaptiveCertificationProposal(AdaptiveCertificationProposal item)
    {
        return AttestationCheckpointPolicy.AttestationCheckpointBucket(item.ProjectedLoad());
    }
}
