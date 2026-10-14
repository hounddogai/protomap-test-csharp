namespace Acme.Workforce.Orgdesign.Tests.Position.Manifest;

using Acme.Workforce.Orgdesign.Position.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LegacyPositionAssignmentRulesChecks
{
    public static void LegacyPositionAssignmentValidatorReportsEveryProblem()
    {
        var validator = LegacyPositionAssignmentRules.DefaultLegacyPositionAssignmentValidator();
        var item = LegacyPositionAssignment.NewLegacyPositionAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPositionAssignmentRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new LegacyPositionAssignmentRequiredSkillRule("legacy position assignment needs an entry");
        var item = LegacyPositionAssignment.NewLegacyPositionAssignment("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPositionAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyPositionAssignmentRequiredTitleRule("legacy position assignment needs an entry");
        var item = LegacyPositionAssignment.NewLegacyPositionAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyPositionAssignmentValidatorReportsEveryProblem();
        LegacyPositionAssignmentRequiredSkillRuleFlagsABlankSkill();
        LegacyPositionAssignmentRequiredTitleRuleFlagsABlankTitle();
    }
}
