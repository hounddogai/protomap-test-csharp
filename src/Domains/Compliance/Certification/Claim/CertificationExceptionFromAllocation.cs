namespace Acme.Workforce.Compliance.Certification.Claim;

using Acme.Workforce.Compliance.Attestation.Batch;
using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Functions of certification exception from allocation.</summary>
public static class CertificationExceptionFromAllocation
{
    public static Dictionary<string, double> SummarizePolicyAckAllocationsForException(List<PolicyAckAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<CertificationException> DeriveCertificationExceptionsFromAllocation(List<PolicyAckAllocation> items)
    {
        var outValue = new List<CertificationException>();
        foreach (var item in items)
        {
            var derived = CertificationException.NewCertificationException(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchCertificationException(CertificationException item)
    {
        return WeightedPolicyAckBundlePolicy.ClassifyWeightedPolicyAckBundle(item.AdjustedVolume());
    }

    public static Dictionary<string, double> SummarizeDefaultAttestationAllocationsForException(List<DefaultAttestationAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<CertificationException> DeriveCertificationExceptionsFromAllocationExtra(List<DefaultAttestationAllocation> items)
    {
        var outValue = new List<CertificationException>();
        foreach (var item in items)
        {
            var derived = CertificationException.NewCertificationException(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreCertificationException(CertificationException item)
    {
        return AttestationBatchPolicy.AttestationBatchWeight(5, true);
    }
}
