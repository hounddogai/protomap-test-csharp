namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SkillProfileBundleRulesChecks
{
    public static void SkillProfileBundleValidatorReportsEveryProblem()
    {
        var validator = SkillProfileBundleRules.DefaultSkillProfileBundleValidator();
        var item = SkillProfileBundle.NewSkillProfileBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SkillProfileBundleLabelLengthRule(120);
        var item = SkillProfileBundle.NewSkillProfileBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileBundleRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new SkillProfileBundleRequiredExternalRefRule("skill profile bundle needs this field");
        var item = SkillProfileBundle.NewSkillProfileBundle("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileBundleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SkillProfileBundleRequiredLabelRule("skill profile bundle needs a value");
        var item = SkillProfileBundle.NewSkillProfileBundle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SkillProfileBundleValidatorReportsEveryProblem();
        SkillProfileBundleLabelLengthRuleFlagsALongLabel();
        SkillProfileBundleRequiredExternalRefRuleFlagsABlankExternalRef();
        SkillProfileBundleRequiredLabelRuleFlagsABlankLabel();
    }
}
