namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredSkillProfileCycleViewChecks
{
    public static void TieredSkillProfileCycleViewShowsTheStatusInCapitals()
    {
        var item = TieredSkillProfileCycle.NewTieredSkillProfileCycle("code-20", "label-827");
        var view = TieredSkillProfileCycleView.ToTieredSkillProfileCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredSkillProfileCycleView.ToTieredSkillProfileCycleViews(new List<TieredSkillProfileCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredSkillProfileCycleViewShowsTheStatusInCapitals();
    }
}
