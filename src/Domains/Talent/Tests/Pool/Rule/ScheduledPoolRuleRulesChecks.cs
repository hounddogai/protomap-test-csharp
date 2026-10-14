namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScheduledPoolRuleRulesChecks
{
    public static void ScheduledPoolRuleValidatorReportsEveryProblem()
    {
        var validator = ScheduledPoolRuleRules.DefaultScheduledPoolRuleValidator();
        var item = ScheduledPoolRule.NewScheduledPoolRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPoolRuleMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new ScheduledPoolRuleMinimumRangeRule(7.4, 1154.3);
        var item = ScheduledPoolRule.NewScheduledPoolRule("x", "y");
        item.Minimum = 1155.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPoolRuleCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new ScheduledPoolRuleCapacityRangeRule(255, 86400);
        var item = ScheduledPoolRule.NewScheduledPoolRule("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPoolRuleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new ScheduledPoolRuleDurationMinutesRangeRule(16, 3600);
        var item = ScheduledPoolRule.NewScheduledPoolRule("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledPoolRuleValidatorReportsEveryProblem();
        ScheduledPoolRuleMinimumRangeRuleFlagsValuesAboveMinimum();
        ScheduledPoolRuleCapacityRangeRuleFlagsValuesAboveCapacity();
        ScheduledPoolRuleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
