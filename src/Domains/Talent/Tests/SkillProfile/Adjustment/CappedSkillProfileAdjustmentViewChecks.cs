namespace Acme.Workforce.Talent.Tests.SkillProfile.Adjustment;

using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedSkillProfileAdjustmentViewChecks
{
    public static void CappedSkillProfileAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = CappedSkillProfileAdjustment.NewCappedSkillProfileAdjustment("code-362", "name-317");
        var view = CappedSkillProfileAdjustmentView.ToCappedSkillProfileAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedSkillProfileAdjustmentView.ToCappedSkillProfileAdjustmentViews(new List<CappedSkillProfileAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedSkillProfileAdjustmentViewShowsTheStatusInCapitals();
    }
}
