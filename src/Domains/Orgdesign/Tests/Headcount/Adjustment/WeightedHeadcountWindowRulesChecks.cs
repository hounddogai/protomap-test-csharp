namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedHeadcountWindowRulesChecks
{
    public static void WeightedHeadcountWindowValidatorReportsEveryProblem()
    {
        var validator = WeightedHeadcountWindowRules.DefaultWeightedHeadcountWindowValidator();
        var item = WeightedHeadcountWindow.NewWeightedHeadcountWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedHeadcountWindowWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new WeightedHeadcountWindowWeightRangeRule(9.5, 1105.4);
        var item = WeightedHeadcountWindow.NewWeightedHeadcountWindow("x", "y");
        item.Weight = 1106.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedHeadcountWindowRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new WeightedHeadcountWindowRequiredManagerIdRule("weighted headcount window needs a value");
        var item = WeightedHeadcountWindow.NewWeightedHeadcountWindow("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedHeadcountWindowManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new WeightedHeadcountWindowManagerIdLengthRule(255);
        var item = WeightedHeadcountWindow.NewWeightedHeadcountWindow("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedHeadcountWindowAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new WeightedHeadcountWindowAttemptsRangeRule(16, 32);
        var item = WeightedHeadcountWindow.NewWeightedHeadcountWindow("x", "y");
        item.Attempts = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedHeadcountWindowValidatorReportsEveryProblem();
        WeightedHeadcountWindowWeightRangeRuleFlagsValuesAboveWeight();
        WeightedHeadcountWindowRequiredManagerIdRuleFlagsABlankManagerId();
        WeightedHeadcountWindowManagerIdLengthRuleFlagsALongManagerId();
        WeightedHeadcountWindowAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
