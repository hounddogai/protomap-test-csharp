namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using Acme.Workforce.Talent.Nomination.Notice;

/// <summary>Functions of global career path profile from notice.</summary>
public static class GlobalCareerPathProfileFromNotice
{
    public static Dictionary<string, double> SummarizeNominationNoticesForProfile(List<NominationNotice> items)
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

    public static List<GlobalCareerPathProfile> DeriveGlobalCareerPathProfilesFromNotice(List<NominationNotice> items)
    {
        var outValue = new List<GlobalCareerPathProfile>();
        foreach (var item in items)
        {
            var derived = GlobalCareerPathProfile.NewGlobalCareerPathProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectGlobalCareerPathProfile(GlobalCareerPathProfile item)
    {
        return NominationNoticePolicy.NominationNoticeBucket(item.NetLoad());
    }
}
