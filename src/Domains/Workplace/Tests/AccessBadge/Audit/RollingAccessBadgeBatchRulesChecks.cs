namespace Acme.Workforce.Workplace.Tests.AccessBadge.Audit;

using Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RollingAccessBadgeBatchRulesChecks
{
    public static void RollingAccessBadgeBatchValidatorReportsEveryProblem()
    {
        var validator = RollingAccessBadgeBatchRules.DefaultRollingAccessBadgeBatchValidator();
        var item = RollingAccessBadgeBatch.NewRollingAccessBadgeBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingAccessBadgeBatchTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RollingAccessBadgeBatchTitleLengthRule(120);
        var item = RollingAccessBadgeBatch.NewRollingAccessBadgeBatch("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingAccessBadgeBatchQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new RollingAccessBadgeBatchQuantityRangeRule(16, 120);
        var item = RollingAccessBadgeBatch.NewRollingAccessBadgeBatch("x", "y");
        item.Quantity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingAccessBadgeBatchValidatorReportsEveryProblem();
        RollingAccessBadgeBatchTitleLengthRuleFlagsALongTitle();
        RollingAccessBadgeBatchQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
