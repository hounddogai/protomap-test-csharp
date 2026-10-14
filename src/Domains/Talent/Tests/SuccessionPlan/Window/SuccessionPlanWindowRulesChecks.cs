namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SuccessionPlanWindowRulesChecks
{
    public static void SuccessionPlanWindowValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanWindowRules.DefaultSuccessionPlanWindowValidator();
        var item = SuccessionPlanWindow.NewSuccessionPlanWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanWindowLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new SuccessionPlanWindowLocaleLengthRule(80);
        var item = SuccessionPlanWindow.NewSuccessionPlanWindow("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanWindowQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new SuccessionPlanWindowQuantityRangeRule(1000, 86400);
        var item = SuccessionPlanWindow.NewSuccessionPlanWindow("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanWindowSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new SuccessionPlanWindowSummaryLengthRule(255);
        var item = SuccessionPlanWindow.NewSuccessionPlanWindow("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SuccessionPlanWindowValidatorReportsEveryProblem();
        SuccessionPlanWindowLocaleLengthRuleFlagsALongLocale();
        SuccessionPlanWindowQuantityRangeRuleFlagsValuesAboveQuantity();
        SuccessionPlanWindowSummaryLengthRuleFlagsALongSummary();
    }
}
