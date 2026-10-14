namespace Acme.Workforce.Talent.SuccessionPlan.Request;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Functions of succession plan digest from reservation.</summary>
public static class SuccessionPlanDigestFromReservation
{
    public static Dictionary<string, double> SummarizeSkillProfileReservationsForDigest(List<SkillProfileReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<SuccessionPlanDigest> DeriveSuccessionPlanDigestsFromReservation(List<SkillProfileReservation> items)
    {
        var outValue = new List<SuccessionPlanDigest>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanDigest.NewSuccessionPlanDigest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeSuccessionPlanDigest(SuccessionPlanDigest item)
    {
        return SkillProfileReservationPolicy.NormalizeSkillProfileReservationCode(item.Code);
    }
}
