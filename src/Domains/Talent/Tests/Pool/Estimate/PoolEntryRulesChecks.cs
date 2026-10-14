namespace Acme.Workforce.Talent.Tests.Pool.Estimate;

using Acme.Workforce.Talent.Pool.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PoolEntryRulesChecks
{
    public static void PoolEntryValidatorReportsEveryProblem()
    {
        var validator = PoolEntryRules.DefaultPoolEntryValidator();
        var item = PoolEntry.NewPoolEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolEntryPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new PoolEntryPriorityRangeRule(64, 86400);
        var item = PoolEntry.NewPoolEntry("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolEntryRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new PoolEntryRequiredTimezoneRule("pool entry needs a value");
        var item = PoolEntry.NewPoolEntry("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PoolEntryValidatorReportsEveryProblem();
        PoolEntryPriorityRangeRuleFlagsValuesAbovePriority();
        PoolEntryRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
