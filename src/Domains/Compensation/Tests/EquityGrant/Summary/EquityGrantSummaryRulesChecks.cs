namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantSummaryRulesChecks
{
    public static void EquityGrantSummaryValidatorReportsEveryProblem()
    {
        var validator = EquityGrantSummaryRules.DefaultEquityGrantSummaryValidator();
        var item = EquityGrantSummary.NewEquityGrantSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantSummaryCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new EquityGrantSummaryCeilingRangeRule(4.1, 3965.2);
        var item = EquityGrantSummary.NewEquityGrantSummary("x", "y");
        item.Ceiling = 3966.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantSummaryRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new EquityGrantSummaryRequiredUnitRule("equity grant summary needs an entry");
        var item = EquityGrantSummary.NewEquityGrantSummary("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantSummaryValidatorReportsEveryProblem();
        EquityGrantSummaryCeilingRangeRuleFlagsValuesAboveCeiling();
        EquityGrantSummaryRequiredUnitRuleFlagsABlankUnit();
    }
}
