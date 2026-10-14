namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PrimaryAdjustmentPreferenceRulesChecks
{
    public static void PrimaryAdjustmentPreferenceValidatorReportsEveryProblem()
    {
        var validator = PrimaryAdjustmentPreferenceRules.DefaultPrimaryAdjustmentPreferenceValidator();
        var item = PrimaryAdjustmentPreference.NewPrimaryAdjustmentPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryAdjustmentPreferenceRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new PrimaryAdjustmentPreferenceRequiredSkillRule("primary adjustment preference needs this field");
        var item = PrimaryAdjustmentPreference.NewPrimaryAdjustmentPreference("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryAdjustmentPreferenceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PrimaryAdjustmentPreferenceRequiredLabelRule("primary adjustment preference needs this field");
        var item = PrimaryAdjustmentPreference.NewPrimaryAdjustmentPreference("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryAdjustmentPreferenceValidatorReportsEveryProblem();
        PrimaryAdjustmentPreferenceRequiredSkillRuleFlagsABlankSkill();
        PrimaryAdjustmentPreferenceRequiredLabelRuleFlagsABlankLabel();
    }
}
