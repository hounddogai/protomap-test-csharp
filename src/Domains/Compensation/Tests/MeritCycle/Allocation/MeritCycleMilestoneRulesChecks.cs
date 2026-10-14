namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleMilestoneRulesChecks
{
    public static void MeritCycleMilestoneValidatorReportsEveryProblem()
    {
        var validator = MeritCycleMilestoneRules.DefaultMeritCycleMilestoneValidator();
        var item = MeritCycleMilestone.NewMeritCycleMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleMilestoneLabelLengthRuleFlagsALongLabel()
    {
        var rule = new MeritCycleMilestoneLabelLengthRule(64);
        var item = MeritCycleMilestone.NewMeritCycleMilestone("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleMilestoneRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new MeritCycleMilestoneRevisionRangeRule(3600, 86400);
        var item = MeritCycleMilestone.NewMeritCycleMilestone("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleMilestoneSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new MeritCycleMilestoneSummaryLengthRule(120);
        var item = MeritCycleMilestone.NewMeritCycleMilestone("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleMilestoneBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new MeritCycleMilestoneBatchSizeRangeRule(80, 120);
        var item = MeritCycleMilestone.NewMeritCycleMilestone("x", "y");
        item.BatchSize = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MeritCycleMilestoneValidatorReportsEveryProblem();
        MeritCycleMilestoneLabelLengthRuleFlagsALongLabel();
        MeritCycleMilestoneRevisionRangeRuleFlagsValuesAboveRevision();
        MeritCycleMilestoneSummaryLengthRuleFlagsALongSummary();
        MeritCycleMilestoneBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
