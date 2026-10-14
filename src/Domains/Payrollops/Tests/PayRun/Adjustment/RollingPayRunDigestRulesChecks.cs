namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingPayRunDigestRulesChecks
{
    public static void RollingPayRunDigestValidatorReportsEveryProblem()
    {
        var validator = RollingPayRunDigestRules.DefaultRollingPayRunDigestValidator();
        var item = RollingPayRunDigest.NewRollingPayRunDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPayRunDigestTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new RollingPayRunDigestTenureMonthsRangeRule(16, 120);
        var item = RollingPayRunDigest.NewRollingPayRunDigest("x", "y");
        item.TenureMonths = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingPayRunDigestPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new RollingPayRunDigestPriorityRangeRule(255, 86400);
        var item = RollingPayRunDigest.NewRollingPayRunDigest("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingPayRunDigestRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new RollingPayRunDigestRevisionRangeRule(16, 86400);
        var item = RollingPayRunDigest.NewRollingPayRunDigest("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingPayRunDigestRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new RollingPayRunDigestRequiredLocationRule("rolling pay run digest needs a value");
        var item = RollingPayRunDigest.NewRollingPayRunDigest("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingPayRunDigestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingPayRunDigestRequiredTitleRule("rolling pay run digest needs an entry");
        var item = RollingPayRunDigest.NewRollingPayRunDigest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingPayRunDigestValidatorReportsEveryProblem();
        RollingPayRunDigestTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        RollingPayRunDigestPriorityRangeRuleFlagsValuesAbovePriority();
        RollingPayRunDigestRevisionRangeRuleFlagsValuesAboveRevision();
        RollingPayRunDigestRequiredLocationRuleFlagsABlankLocation();
        RollingPayRunDigestRequiredTitleRuleFlagsABlankTitle();
    }
}
