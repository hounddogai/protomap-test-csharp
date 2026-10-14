namespace Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Functions of legacy skill profile preference policy.</summary>
public static class LegacySkillProfilePreferencePolicy
{
    public static int LegacySkillProfilePreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 13714.0, in order.</summary>
    public static int LegacySkillProfilePreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13714.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
