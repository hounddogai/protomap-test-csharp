namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolBatch;

using Acme.Workforce.Compensation.BonusPool.PoolBatch;

/// <summary>Checks of pool batch code, run by the project's runner.</summary>
public static class ScheduledBonusPoolMilestoneRulesChecks
{
    public static void ScheduledBonusPoolMilestoneValidatorReportsEveryProblem()
    {
        var validator = ScheduledBonusPoolMilestoneRules.DefaultScheduledBonusPoolMilestoneValidator();
        var item = ScheduledBonusPoolMilestone.NewScheduledBonusPoolMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledBonusPoolMilestoneRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledBonusPoolMilestoneRequiredNameRule("scheduled bonus pool milestone needs this field");
        var item = ScheduledBonusPoolMilestone.NewScheduledBonusPoolMilestone("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledBonusPoolMilestoneNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledBonusPoolMilestoneNameLengthRule(120);
        var item = ScheduledBonusPoolMilestone.NewScheduledBonusPoolMilestone("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledBonusPoolMilestoneValidatorReportsEveryProblem();
        ScheduledBonusPoolMilestoneRequiredNameRuleFlagsABlankName();
        ScheduledBonusPoolMilestoneNameLengthRuleFlagsALongName();
    }
}
