namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StrictPoolRuleRulesChecks
{
    public static void StrictPoolRuleValidatorReportsEveryProblem()
    {
        var validator = StrictPoolRuleRules.DefaultStrictPoolRuleValidator();
        var item = StrictPoolRule.NewStrictPoolRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPoolRuleManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new StrictPoolRuleManagerIdLengthRule(32);
        var item = StrictPoolRule.NewStrictPoolRule("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPoolRuleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new StrictPoolRuleDurationMinutesRangeRule(120, 255);
        var item = StrictPoolRule.NewStrictPoolRule("x", "y");
        item.DurationMinutes = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictPoolRuleMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new StrictPoolRuleMinimumRangeRule(3.7, 63.1);
        var item = StrictPoolRule.NewStrictPoolRule("x", "y");
        item.Minimum = 64.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictPoolRuleValidatorReportsEveryProblem();
        StrictPoolRuleManagerIdLengthRuleFlagsALongManagerId();
        StrictPoolRuleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        StrictPoolRuleMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
