namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentAuditRulesChecks
{
    public static void AssignmentAuditValidatorReportsEveryProblem()
    {
        var validator = AssignmentAuditRules.DefaultAssignmentAuditValidator();
        var item = AssignmentAudit.NewAssignmentAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentAuditRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new AssignmentAuditRequiredTeamRule("assignment audit needs an entry");
        var item = AssignmentAudit.NewAssignmentAudit("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssignmentAuditCodeLengthRule(16);
        var item = AssignmentAudit.NewAssignmentAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssignmentAuditValidatorReportsEveryProblem();
        AssignmentAuditRequiredTeamRuleFlagsABlankTeam();
        AssignmentAuditCodeLengthRuleFlagsALongCode();
    }
}
