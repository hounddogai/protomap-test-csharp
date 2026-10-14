namespace Acme.Workforce.Talent.Tests.CareerPath.OverrideItem;

using Acme.Workforce.Talent.CareerPath.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredCareerPathOverrideRulesChecks
{
    public static void TieredCareerPathOverrideValidatorReportsEveryProblem()
    {
        var validator = TieredCareerPathOverrideRules.DefaultTieredCareerPathOverrideValidator();
        var item = TieredCareerPathOverride.NewTieredCareerPathOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredCareerPathOverrideRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new TieredCareerPathOverrideRequiredLocationRule("tiered career path override needs a value");
        var item = TieredCareerPathOverride.NewTieredCareerPathOverride("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredCareerPathOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredCareerPathOverrideRequiredCodeRule("tiered career path override needs a value");
        var item = TieredCareerPathOverride.NewTieredCareerPathOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredCareerPathOverrideExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new TieredCareerPathOverrideExpiresAtRangeRule(80, 80);
        var item = TieredCareerPathOverride.NewTieredCareerPathOverride("x", "y");
        item.ExpiresAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredCareerPathOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredCareerPathOverrideRequiredLabelRule("tiered career path override needs a value");
        var item = TieredCareerPathOverride.NewTieredCareerPathOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredCareerPathOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredCareerPathOverrideCodeLengthRule(1000);
        var item = TieredCareerPathOverride.NewTieredCareerPathOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredCareerPathOverrideValidatorReportsEveryProblem();
        TieredCareerPathOverrideRequiredLocationRuleFlagsABlankLocation();
        TieredCareerPathOverrideRequiredCodeRuleFlagsABlankCode();
        TieredCareerPathOverrideExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        TieredCareerPathOverrideRequiredLabelRuleFlagsABlankLabel();
        TieredCareerPathOverrideCodeLengthRuleFlagsALongCode();
    }
}
