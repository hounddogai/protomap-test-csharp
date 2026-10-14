namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Functions of pending skill profile group from agreement.</summary>
public static class PendingSkillProfileGroupFromAgreement
{
    public static Dictionary<string, double> SummarizeNominationAgreementsForGroup(List<NominationAgreement> items)
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

    public static List<PendingSkillProfileGroup> DerivePendingSkillProfileGroupsFromAgreement(List<NominationAgreement> items)
    {
        var outValue = new List<PendingSkillProfileGroup>();
        foreach (var item in items)
        {
            var derived = PendingSkillProfileGroup.NewPendingSkillProfileGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyPendingSkillProfileGroup(PendingSkillProfileGroup item)
    {
        return GlobalNominationSnapshotPolicy.ClassifyGlobalNominationSnapshot(item.ProjectedValue());
    }
}
