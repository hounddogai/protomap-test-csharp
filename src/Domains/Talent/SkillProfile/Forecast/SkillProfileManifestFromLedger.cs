namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Talent.Pool.Group;

/// <summary>Functions of skill profile manifest from ledger.</summary>
public static class SkillProfileManifestFromLedger
{
    public static Dictionary<string, double> SummarizeRollingPoolLedgersForManifest(List<RollingPoolLedger> items)
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

    public static List<SkillProfileManifest> DeriveSkillProfileManifestsFromLedger(List<RollingPoolLedger> items)
    {
        var outValue = new List<SkillProfileManifest>();
        foreach (var item in items)
        {
            var derived = SkillProfileManifest.NewSkillProfileManifest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchSkillProfileManifest(SkillProfileManifest item)
    {
        return PoolLedgerPolicy.NormalizePoolLedgerCode(item.Code);
    }
}
