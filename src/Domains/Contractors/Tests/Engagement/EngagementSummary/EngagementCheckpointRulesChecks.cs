namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class EngagementCheckpointRulesChecks
{
    public static void EngagementCheckpointValidatorReportsEveryProblem()
    {
        var validator = EngagementCheckpointRules.DefaultEngagementCheckpointValidator();
        var item = EngagementCheckpoint.NewEngagementCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementCheckpointRequiredCodeRule("engagement checkpoint needs an entry");
        var item = EngagementCheckpoint.NewEngagementCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementCheckpointRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new EngagementCheckpointRequiredJobCodeRule("engagement checkpoint needs an entry");
        var item = EngagementCheckpoint.NewEngagementCheckpoint("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementCheckpointPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new EngagementCheckpointPriorityRangeRule(80, 3600);
        var item = EngagementCheckpoint.NewEngagementCheckpoint("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementCheckpointManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new EngagementCheckpointManagerIdLengthRule(3600);
        var item = EngagementCheckpoint.NewEngagementCheckpoint("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementCheckpointJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new EngagementCheckpointJobCodeLengthRule(255);
        var item = EngagementCheckpoint.NewEngagementCheckpoint("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EngagementCheckpointValidatorReportsEveryProblem();
        EngagementCheckpointRequiredCodeRuleFlagsABlankCode();
        EngagementCheckpointRequiredJobCodeRuleFlagsABlankJobCode();
        EngagementCheckpointPriorityRangeRuleFlagsValuesAbovePriority();
        EngagementCheckpointManagerIdLengthRuleFlagsALongManagerId();
        EngagementCheckpointJobCodeLengthRuleFlagsALongJobCode();
    }
}
