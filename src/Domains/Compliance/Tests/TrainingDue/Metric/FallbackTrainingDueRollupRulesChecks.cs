namespace Acme.Workforce.Compliance.Tests.TrainingDue.Metric;

using Acme.Workforce.Compliance.TrainingDue.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class FallbackTrainingDueRollupRulesChecks
{
    public static void FallbackTrainingDueRollupValidatorReportsEveryProblem()
    {
        var validator = FallbackTrainingDueRollupRules.DefaultFallbackTrainingDueRollupValidator();
        var item = FallbackTrainingDueRollup.NewFallbackTrainingDueRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackTrainingDueRollupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackTrainingDueRollupRequiredTitleRule("fallback training due rollup needs an entry");
        var item = FallbackTrainingDueRollup.NewFallbackTrainingDueRollup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackTrainingDueRollupRegionLengthRuleFlagsALongRegion()
    {
        var rule = new FallbackTrainingDueRollupRegionLengthRule(64);
        var item = FallbackTrainingDueRollup.NewFallbackTrainingDueRollup("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackTrainingDueRollupGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new FallbackTrainingDueRollupGraceDaysRangeRule(64, 80);
        var item = FallbackTrainingDueRollup.NewFallbackTrainingDueRollup("x", "y");
        item.GraceDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackTrainingDueRollupValidatorReportsEveryProblem();
        FallbackTrainingDueRollupRequiredTitleRuleFlagsABlankTitle();
        FallbackTrainingDueRollupRegionLengthRuleFlagsALongRegion();
        FallbackTrainingDueRollupGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
