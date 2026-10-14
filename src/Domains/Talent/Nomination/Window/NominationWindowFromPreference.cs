namespace Acme.Workforce.Talent.Nomination.Window;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Functions of nomination window from preference.</summary>
public static class NominationWindowFromPreference
{
    public static Dictionary<string, double> SummarizeLegacySkillProfilePreferencesForWindow(List<LegacySkillProfilePreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<NominationWindow> DeriveNominationWindowsFromPreference(List<LegacySkillProfilePreference> items)
    {
        var outValue = new List<NominationWindow>();
        foreach (var item in items)
        {
            var derived = NominationWindow.NewNominationWindow(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanNominationWindow(NominationWindow item)
    {
        return SkillProfileRecordPolicy.SkillProfileRecordBucket(item.WeightedLoad());
    }
}
