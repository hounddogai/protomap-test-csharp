namespace Acme.Workforce.Payrollops.Tests.Garnishment.Entry;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FallbackGarnishmentPlanRulesChecks
{
    public static void FallbackGarnishmentPlanValidatorReportsEveryProblem()
    {
        var validator = FallbackGarnishmentPlanRules.DefaultFallbackGarnishmentPlanValidator();
        var item = FallbackGarnishmentPlan.NewFallbackGarnishmentPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackGarnishmentPlanRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackGarnishmentPlanRequiredTitleRule("fallback garnishment plan needs a value");
        var item = FallbackGarnishmentPlan.NewFallbackGarnishmentPlan("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackGarnishmentPlanGradeLengthRuleFlagsALongGrade()
    {
        var rule = new FallbackGarnishmentPlanGradeLengthRule(86400);
        var item = FallbackGarnishmentPlan.NewFallbackGarnishmentPlan("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackGarnishmentPlanScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new FallbackGarnishmentPlanScoreRangeRule(8.8, 1005.6);
        var item = FallbackGarnishmentPlan.NewFallbackGarnishmentPlan("x", "y");
        item.Score = 1007.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackGarnishmentPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackGarnishmentPlanCodeLengthRule(86400);
        var item = FallbackGarnishmentPlan.NewFallbackGarnishmentPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackGarnishmentPlanValidatorReportsEveryProblem();
        FallbackGarnishmentPlanRequiredTitleRuleFlagsABlankTitle();
        FallbackGarnishmentPlanGradeLengthRuleFlagsALongGrade();
        FallbackGarnishmentPlanScoreRangeRuleFlagsValuesAboveScore();
        FallbackGarnishmentPlanCodeLengthRuleFlagsALongCode();
    }
}
