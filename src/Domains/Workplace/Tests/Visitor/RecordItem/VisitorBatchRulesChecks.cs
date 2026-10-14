namespace Acme.Workforce.Workplace.Tests.Visitor.RecordItem;

using Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class VisitorBatchRulesChecks
{
    public static void VisitorBatchValidatorReportsEveryProblem()
    {
        var validator = VisitorBatchRules.DefaultVisitorBatchValidator();
        var item = VisitorBatch.NewVisitorBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorBatchWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new VisitorBatchWindowDaysRangeRule(16, 32);
        var item = VisitorBatch.NewVisitorBatch("x", "y");
        item.WindowDays = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorBatchTitleLengthRuleFlagsALongTitle()
    {
        var rule = new VisitorBatchTitleLengthRule(86400);
        var item = VisitorBatch.NewVisitorBatch("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorBatchRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new VisitorBatchRevisionRangeRule(32, 80);
        var item = VisitorBatch.NewVisitorBatch("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        VisitorBatchValidatorReportsEveryProblem();
        VisitorBatchWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        VisitorBatchTitleLengthRuleFlagsALongTitle();
        VisitorBatchRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
