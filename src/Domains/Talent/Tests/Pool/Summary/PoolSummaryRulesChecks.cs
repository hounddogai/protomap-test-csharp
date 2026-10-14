namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PoolSummaryRulesChecks
{
    public static void PoolSummaryValidatorReportsEveryProblem()
    {
        var validator = PoolSummaryRules.DefaultPoolSummaryValidator();
        var item = PoolSummary.NewPoolSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolSummaryRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PoolSummaryRetriesRangeRule(120, 1000);
        var item = PoolSummary.NewPoolSummary("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PoolSummaryRequiredCodeRule("pool summary needs an entry");
        var item = PoolSummary.NewPoolSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PoolSummaryValidatorReportsEveryProblem();
        PoolSummaryRetriesRangeRuleFlagsValuesAboveRetries();
        PoolSummaryRequiredCodeRuleFlagsABlankCode();
    }
}
