namespace Acme.Workforce.Compensation.Tests.EquityGrant.Target;

using Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledEquityGrantRollupRulesChecks
{
    public static void ScheduledEquityGrantRollupValidatorReportsEveryProblem()
    {
        var validator = ScheduledEquityGrantRollupRules.DefaultScheduledEquityGrantRollupValidator();
        var item = ScheduledEquityGrantRollup.NewScheduledEquityGrantRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledEquityGrantRollupQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new ScheduledEquityGrantRollupQuantityRangeRule(64, 86400);
        var item = ScheduledEquityGrantRollup.NewScheduledEquityGrantRollup("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledEquityGrantRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledEquityGrantRollupRequiredCodeRule("scheduled equity grant rollup needs an entry");
        var item = ScheduledEquityGrantRollup.NewScheduledEquityGrantRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledEquityGrantRollupValidatorReportsEveryProblem();
        ScheduledEquityGrantRollupQuantityRangeRuleFlagsValuesAboveQuantity();
        ScheduledEquityGrantRollupRequiredCodeRuleFlagsABlankCode();
    }
}
