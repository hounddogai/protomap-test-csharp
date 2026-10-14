namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Functions of succession plan profile from entry.</summary>
public static class SuccessionPlanProfileFromEntry
{
    public static Dictionary<string, double> SummarizeLegacyCareerPathEntriesForProfile(List<LegacyCareerPathEntry> items)
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

    public static List<SuccessionPlanProfile> DeriveSuccessionPlanProfilesFromEntry(List<LegacyCareerPathEntry> items)
    {
        var outValue = new List<SuccessionPlanProfile>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanProfile.NewSuccessionPlanProfile(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileSuccessionPlanProfile(SuccessionPlanProfile item)
    {
        return LegacyCareerPathEntryPolicy.NormalizeLegacyCareerPathEntryCode(item.Code);
    }
}
