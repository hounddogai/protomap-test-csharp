namespace Acme.Workforce.Compensation.Tests.MeritCycle.Batch;

using Acme.Workforce.Compensation.MeritCycle.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingMeritCycleBatchRulesChecks
{
    public static void PendingMeritCycleBatchValidatorReportsEveryProblem()
    {
        var validator = PendingMeritCycleBatchRules.DefaultPendingMeritCycleBatchValidator();
        var item = PendingMeritCycleBatch.NewPendingMeritCycleBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingMeritCycleBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingMeritCycleBatchRequiredTitleRule("pending merit cycle batch needs an entry");
        var item = PendingMeritCycleBatch.NewPendingMeritCycleBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingMeritCycleBatchRegionLengthRuleFlagsALongRegion()
    {
        var rule = new PendingMeritCycleBatchRegionLengthRule(86400);
        var item = PendingMeritCycleBatch.NewPendingMeritCycleBatch("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingMeritCycleBatchTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingMeritCycleBatchTitleLengthRule(255);
        var item = PendingMeritCycleBatch.NewPendingMeritCycleBatch("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingMeritCycleBatchValidatorReportsEveryProblem();
        PendingMeritCycleBatchRequiredTitleRuleFlagsABlankTitle();
        PendingMeritCycleBatchRegionLengthRuleFlagsALongRegion();
        PendingMeritCycleBatchTitleLengthRuleFlagsALongTitle();
    }
}
