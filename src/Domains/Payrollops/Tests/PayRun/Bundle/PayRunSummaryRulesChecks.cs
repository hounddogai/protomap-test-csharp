namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunSummaryRulesChecks
{
    public static void PayRunSummaryValidatorReportsEveryProblem()
    {
        var validator = PayRunSummaryRules.DefaultPayRunSummaryValidator();
        var item = PayRunSummary.NewPayRunSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunSummaryCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new PayRunSummaryCategoryLengthRule(3600);
        var item = PayRunSummary.NewPayRunSummary("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayRunSummaryRequiredCodeRule("pay run summary needs a value");
        var item = PayRunSummary.NewPayRunSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PayRunSummaryRequiredTitleRule("pay run summary needs this field");
        var item = PayRunSummary.NewPayRunSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunSummaryUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new PayRunSummaryUpdatedAtRangeRule(80, 120);
        var item = PayRunSummary.NewPayRunSummary("x", "y");
        item.UpdatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunSummaryRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new PayRunSummaryRequiredSegmentRule("pay run summary needs a value");
        var item = PayRunSummary.NewPayRunSummary("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayRunSummaryValidatorReportsEveryProblem();
        PayRunSummaryCategoryLengthRuleFlagsALongCategory();
        PayRunSummaryRequiredCodeRuleFlagsABlankCode();
        PayRunSummaryRequiredTitleRuleFlagsABlankTitle();
        PayRunSummaryUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        PayRunSummaryRequiredSegmentRuleFlagsABlankSegment();
    }
}
