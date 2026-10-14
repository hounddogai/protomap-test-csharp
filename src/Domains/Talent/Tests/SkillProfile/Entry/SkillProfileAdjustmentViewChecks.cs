namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SkillProfileAdjustmentViewChecks
{
    public static void SkillProfileAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileAdjustment.NewSkillProfileAdjustment("code-246", "label-473");
        var view = SkillProfileAdjustmentView.ToSkillProfileAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileAdjustmentView.ToSkillProfileAdjustmentViews(new List<SkillProfileAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileAdjustmentViewShowsTheStatusInCapitals();
    }
}
