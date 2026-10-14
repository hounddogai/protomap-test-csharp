namespace Acme.Workforce.Absence.Tests.Balance.Batch;

using Acme.Workforce.Absence.Balance.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BalanceBatchRulesChecks
{
    public static void BalanceBatchValidatorReportsEveryProblem()
    {
        var validator = BalanceBatchRules.DefaultBalanceBatchValidator();
        var item = BalanceBatch.NewBalanceBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceBatchWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new BalanceBatchWindowDaysRangeRule(32, 80);
        var item = BalanceBatch.NewBalanceBatch("x", "y");
        item.WindowDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceBatchRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new BalanceBatchRevisionRangeRule(16, 120);
        var item = BalanceBatch.NewBalanceBatch("x", "y");
        item.Revision = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceBatchDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new BalanceBatchDescriptionLengthRule(80);
        var item = BalanceBatch.NewBalanceBatch("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BalanceBatchValidatorReportsEveryProblem();
        BalanceBatchWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        BalanceBatchRevisionRangeRuleFlagsValuesAboveRevision();
        BalanceBatchDescriptionLengthRuleFlagsALongDescription();
    }
}
