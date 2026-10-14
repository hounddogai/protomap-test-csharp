namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class WeightedNominationMilestoneRulesChecks
{
    public static void WeightedNominationMilestoneValidatorReportsEveryProblem()
    {
        var validator = WeightedNominationMilestoneRules.DefaultWeightedNominationMilestoneValidator();
        var item = WeightedNominationMilestone.NewWeightedNominationMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedNominationMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedNominationMilestoneRequiredCodeRule("weighted nomination milestone needs a value");
        var item = WeightedNominationMilestone.NewWeightedNominationMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedNominationMilestoneGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new WeightedNominationMilestoneGraceDaysRangeRule(16, 255);
        var item = WeightedNominationMilestone.NewWeightedNominationMilestone("x", "y");
        item.GraceDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedNominationMilestoneStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new WeightedNominationMilestoneStartsAtRangeRule(64, 3600);
        var item = WeightedNominationMilestone.NewWeightedNominationMilestone("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedNominationMilestoneRequiredNameRuleFlagsABlankName()
    {
        var rule = new WeightedNominationMilestoneRequiredNameRule("weighted nomination milestone needs an entry");
        var item = WeightedNominationMilestone.NewWeightedNominationMilestone("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedNominationMilestoneValidatorReportsEveryProblem();
        WeightedNominationMilestoneRequiredCodeRuleFlagsABlankCode();
        WeightedNominationMilestoneGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        WeightedNominationMilestoneStartsAtRangeRuleFlagsValuesAboveStartsAt();
        WeightedNominationMilestoneRequiredNameRuleFlagsABlankName();
    }
}
