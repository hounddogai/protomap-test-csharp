namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Functions of primary skill profile manifest from request.</summary>
public static class PrimarySkillProfileManifestFromRequest
{
    public static Dictionary<string, double> SummarizePoolRequestsForManifest(List<PoolRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<PrimarySkillProfileManifest> DerivePrimarySkillProfileManifestsFromRequest(List<PoolRequest> items)
    {
        var outValue = new List<PrimarySkillProfileManifest>();
        foreach (var item in items)
        {
            var derived = PrimarySkillProfileManifest.NewPrimarySkillProfileManifest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanPrimarySkillProfileManifest(PrimarySkillProfileManifest item)
    {
        return PoolAgreementPolicy.NormalizePoolAgreementCode(item.Code);
    }
}
