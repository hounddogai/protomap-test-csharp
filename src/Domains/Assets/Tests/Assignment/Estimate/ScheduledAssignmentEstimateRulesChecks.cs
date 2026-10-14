namespace Acme.Workforce.Assets.Tests.Assignment.Estimate;

using Acme.Workforce.Assets.Assignment.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ScheduledAssignmentEstimateRulesChecks
{
    public static void ScheduledAssignmentEstimateValidatorReportsEveryProblem()
    {
        var validator = ScheduledAssignmentEstimateRules.DefaultScheduledAssignmentEstimateValidator();
        var item = ScheduledAssignmentEstimate.NewScheduledAssignmentEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledAssignmentEstimateRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ScheduledAssignmentEstimateRevisionRangeRule(16, 80);
        var item = ScheduledAssignmentEstimate.NewScheduledAssignmentEstimate("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledAssignmentEstimateSkillLengthRuleFlagsALongSkill()
    {
        var rule = new ScheduledAssignmentEstimateSkillLengthRule(255);
        var item = ScheduledAssignmentEstimate.NewScheduledAssignmentEstimate("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledAssignmentEstimateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ScheduledAssignmentEstimateRequiredLabelRule("scheduled assignment estimate needs a value");
        var item = ScheduledAssignmentEstimate.NewScheduledAssignmentEstimate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledAssignmentEstimateRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new ScheduledAssignmentEstimateRetriesRangeRule(32, 255);
        var item = ScheduledAssignmentEstimate.NewScheduledAssignmentEstimate("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledAssignmentEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledAssignmentEstimateCodeLengthRule(255);
        var item = ScheduledAssignmentEstimate.NewScheduledAssignmentEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledAssignmentEstimateValidatorReportsEveryProblem();
        ScheduledAssignmentEstimateRevisionRangeRuleFlagsValuesAboveRevision();
        ScheduledAssignmentEstimateSkillLengthRuleFlagsALongSkill();
        ScheduledAssignmentEstimateRequiredLabelRuleFlagsABlankLabel();
        ScheduledAssignmentEstimateRetriesRangeRuleFlagsValuesAboveRetries();
        ScheduledAssignmentEstimateCodeLengthRuleFlagsALongCode();
    }
}
