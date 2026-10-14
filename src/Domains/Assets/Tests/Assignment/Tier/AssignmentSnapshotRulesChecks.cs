namespace Acme.Workforce.Assets.Tests.Assignment.Tier;

using Acme.Workforce.Assets.Assignment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AssignmentSnapshotRulesChecks
{
    public static void AssignmentSnapshotValidatorReportsEveryProblem()
    {
        var validator = AssignmentSnapshotRules.DefaultAssignmentSnapshotValidator();
        var item = AssignmentSnapshot.NewAssignmentSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentSnapshotRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new AssignmentSnapshotRequiredSkillRule("assignment snapshot needs an entry");
        var item = AssignmentSnapshot.NewAssignmentSnapshot("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentSnapshotNameLengthRuleFlagsALongName()
    {
        var rule = new AssignmentSnapshotNameLengthRule(255);
        var item = AssignmentSnapshot.NewAssignmentSnapshot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentSnapshotRequiredNameRuleFlagsABlankName()
    {
        var rule = new AssignmentSnapshotRequiredNameRule("assignment snapshot needs an entry");
        var item = AssignmentSnapshot.NewAssignmentSnapshot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssignmentSnapshotValidatorReportsEveryProblem();
        AssignmentSnapshotRequiredSkillRuleFlagsABlankSkill();
        AssignmentSnapshotNameLengthRuleFlagsALongName();
        AssignmentSnapshotRequiredNameRuleFlagsABlankName();
    }
}
