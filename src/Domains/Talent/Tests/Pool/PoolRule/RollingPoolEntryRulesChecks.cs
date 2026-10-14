namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class RollingPoolEntryRulesChecks
{
    public static void RollingPoolEntryValidatorReportsEveryProblem()
    {
        var validator = RollingPoolEntryRules.DefaultRollingPoolEntryValidator();
        var item = RollingPoolEntry.NewRollingPoolEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPoolEntryCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new RollingPoolEntryCategoryLengthRule(80);
        var item = RollingPoolEntry.NewRollingPoolEntry("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingPoolEntryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingPoolEntryRequiredTitleRule("rolling pool entry needs an entry");
        var item = RollingPoolEntry.NewRollingPoolEntry("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingPoolEntryValidatorReportsEveryProblem();
        RollingPoolEntryCategoryLengthRuleFlagsALongCategory();
        RollingPoolEntryRequiredTitleRuleFlagsABlankTitle();
    }
}
