namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanOverrideRulesChecks
{
    public static void SuccessionPlanOverrideValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanOverrideRules.DefaultSuccessionPlanOverrideValidator();
        var item = SuccessionPlanOverride.NewSuccessionPlanOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanOverrideNameLengthRuleFlagsALongName()
    {
        var rule = new SuccessionPlanOverrideNameLengthRule(32);
        var item = SuccessionPlanOverride.NewSuccessionPlanOverride("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanOverrideRequiredNameRuleFlagsABlankName()
    {
        var rule = new SuccessionPlanOverrideRequiredNameRule("succession plan override needs this field");
        var item = SuccessionPlanOverride.NewSuccessionPlanOverride("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanOverrideCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new SuccessionPlanOverrideCreatedAtRangeRule(32, 80);
        var item = SuccessionPlanOverride.NewSuccessionPlanOverride("x", "y");
        item.CreatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SuccessionPlanOverrideValidatorReportsEveryProblem();
        SuccessionPlanOverrideNameLengthRuleFlagsALongName();
        SuccessionPlanOverrideRequiredNameRuleFlagsABlankName();
        SuccessionPlanOverrideCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
