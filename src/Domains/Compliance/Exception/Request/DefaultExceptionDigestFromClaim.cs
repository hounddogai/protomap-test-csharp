namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>Functions of default exception digest from claim.</summary>
public static class DefaultExceptionDigestFromClaim
{
    public static Dictionary<string, double> SummarizeAttestationClaimsForDigest(List<AttestationClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<DefaultExceptionDigest> DeriveDefaultExceptionDigestsFromClaim(List<AttestationClaim> items)
    {
        var outValue = new List<DefaultExceptionDigest>();
        foreach (var item in items)
        {
            var derived = DefaultExceptionDigest.NewDefaultExceptionDigest(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveDefaultExceptionDigest(DefaultExceptionDigest item)
    {
        return AttestationMilestonePolicy.ClassifyAttestationMilestone(item.NetLoad());
    }
}
