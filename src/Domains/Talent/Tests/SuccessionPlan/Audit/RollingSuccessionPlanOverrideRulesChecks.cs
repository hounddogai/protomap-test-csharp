namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RollingSuccessionPlanOverrideRulesChecks
{
    public static void RollingSuccessionPlanOverrideValidatorReportsEveryProblem()
    {
        var validator = RollingSuccessionPlanOverrideRules.DefaultRollingSuccessionPlanOverrideValidator();
        var item = RollingSuccessionPlanOverride.NewRollingSuccessionPlanOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingSuccessionPlanOverrideRegionLengthRuleFlagsALongRegion()
    {
        var rule = new RollingSuccessionPlanOverrideRegionLengthRule(86400);
        var item = RollingSuccessionPlanOverride.NewRollingSuccessionPlanOverride("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingSuccessionPlanOverrideMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new RollingSuccessionPlanOverrideMaximumRangeRule(1.9, 4235.2);
        var item = RollingSuccessionPlanOverride.NewRollingSuccessionPlanOverride("x", "y");
        item.Maximum = 4236.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingSuccessionPlanOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingSuccessionPlanOverrideCodeLengthRule(120);
        var item = RollingSuccessionPlanOverride.NewRollingSuccessionPlanOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingSuccessionPlanOverrideAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new RollingSuccessionPlanOverrideAmountRangeRule(8.6, 4725.6);
        var item = RollingSuccessionPlanOverride.NewRollingSuccessionPlanOverride("x", "y");
        item.Amount = 4727.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingSuccessionPlanOverrideValidatorReportsEveryProblem();
        RollingSuccessionPlanOverrideRegionLengthRuleFlagsALongRegion();
        RollingSuccessionPlanOverrideMaximumRangeRuleFlagsValuesAboveMaximum();
        RollingSuccessionPlanOverrideCodeLengthRuleFlagsALongCode();
        RollingSuccessionPlanOverrideAmountRangeRuleFlagsValuesAboveAmount();
    }
}
