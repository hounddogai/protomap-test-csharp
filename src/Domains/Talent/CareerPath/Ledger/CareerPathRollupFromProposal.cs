namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Talent.SkillProfile.Score;

/// <summary>Functions of career path rollup from proposal.</summary>
public static class CareerPathRollupFromProposal
{
    public static Dictionary<string, double> SummarizeTieredSkillProfileProposalsForRollup(List<TieredSkillProfileProposal> items)
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

    public static List<CareerPathRollup> DeriveCareerPathRollupsFromProposal(List<TieredSkillProfileProposal> items)
    {
        var outValue = new List<CareerPathRollup>();
        foreach (var item in items)
        {
            var derived = CareerPathRollup.NewCareerPathRollup(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeCareerPathRollup(CareerPathRollup item)
    {
        return SkillProfileExceptionPolicy.SkillProfileExceptionWeight(2, true);
    }
}
