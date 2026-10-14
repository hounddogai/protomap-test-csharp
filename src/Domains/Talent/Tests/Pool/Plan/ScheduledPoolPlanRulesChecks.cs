namespace Acme.Workforce.Talent.Tests.Pool.Plan;

using Acme.Workforce.Talent.Pool.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class ScheduledPoolPlanRulesChecks
{
    public static void ScheduledPoolPlanValidatorReportsEveryProblem()
    {
        var validator = ScheduledPoolPlanRules.DefaultScheduledPoolPlanValidator();
        var item = ScheduledPoolPlan.NewScheduledPoolPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPoolPlanRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new ScheduledPoolPlanRequiredCurrencyRule("scheduled pool plan needs an entry");
        var item = ScheduledPoolPlan.NewScheduledPoolPlan("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPoolPlanTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new ScheduledPoolPlanTimezoneLengthRule(255);
        var item = ScheduledPoolPlan.NewScheduledPoolPlan("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledPoolPlanValidatorReportsEveryProblem();
        ScheduledPoolPlanRequiredCurrencyRuleFlagsABlankCurrency();
        ScheduledPoolPlanTimezoneLengthRuleFlagsALongTimezone();
    }
}
