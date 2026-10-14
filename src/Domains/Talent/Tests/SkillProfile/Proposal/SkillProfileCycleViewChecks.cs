namespace Acme.Workforce.Talent.Tests.SkillProfile.Proposal;

using Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SkillProfileCycleViewChecks
{
    public static void SkillProfileCycleViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileCycle.NewSkillProfileCycle("code-608", "name-23");
        var view = SkillProfileCycleView.ToSkillProfileCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileCycleView.ToSkillProfileCycleViews(new List<SkillProfileCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileCycleViewShowsTheStatusInCapitals();
    }
}
