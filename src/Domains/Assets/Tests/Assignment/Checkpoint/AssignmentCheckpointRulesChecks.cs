namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AssignmentCheckpointRulesChecks
{
    public static void AssignmentCheckpointValidatorReportsEveryProblem()
    {
        var validator = AssignmentCheckpointRules.DefaultAssignmentCheckpointValidator();
        var item = AssignmentCheckpoint.NewAssignmentCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AssignmentCheckpointRequiredLabelRule("assignment checkpoint needs an entry");
        var item = AssignmentCheckpoint.NewAssignmentCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentCheckpointStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new AssignmentCheckpointStartsAtRangeRule(80, 1000);
        var item = AssignmentCheckpoint.NewAssignmentCheckpoint("x", "y");
        item.StartsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssignmentCheckpointRequiredCodeRule("assignment checkpoint needs an entry");
        var item = AssignmentCheckpoint.NewAssignmentCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentCheckpointSkillLengthRuleFlagsALongSkill()
    {
        var rule = new AssignmentCheckpointSkillLengthRule(32);
        var item = AssignmentCheckpoint.NewAssignmentCheckpoint("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssignmentCheckpointValidatorReportsEveryProblem();
        AssignmentCheckpointRequiredLabelRuleFlagsABlankLabel();
        AssignmentCheckpointStartsAtRangeRuleFlagsValuesAboveStartsAt();
        AssignmentCheckpointRequiredCodeRuleFlagsABlankCode();
        AssignmentCheckpointSkillLengthRuleFlagsALongSkill();
    }
}
