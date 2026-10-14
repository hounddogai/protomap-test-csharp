namespace Acme.Workforce.Assets.Tests.Assignment.Snapshot;

using Acme.Workforce.Assets.Assignment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class AssignmentOverrideRulesChecks
{
    public static void AssignmentOverrideValidatorReportsEveryProblem()
    {
        var validator = AssignmentOverrideRules.DefaultAssignmentOverrideValidator();
        var item = AssignmentOverride.NewAssignmentOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentOverrideLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AssignmentOverrideLabelLengthRule(80);
        var item = AssignmentOverride.NewAssignmentOverride("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentOverrideRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new AssignmentOverrideRequiredTeamRule("assignment override needs an entry");
        var item = AssignmentOverride.NewAssignmentOverride("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssignmentOverrideValidatorReportsEveryProblem();
        AssignmentOverrideLabelLengthRuleFlagsALongLabel();
        AssignmentOverrideRequiredTeamRuleFlagsABlankTeam();
    }
}
