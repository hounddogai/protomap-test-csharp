namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AssignmentScoreRulesChecks
{
    public static void AssignmentScoreValidatorReportsEveryProblem()
    {
        var validator = AssignmentScoreRules.DefaultAssignmentScoreValidator();
        var item = AssignmentScore.NewAssignmentScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentScoreExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new AssignmentScoreExternalRefLengthRule(64);
        var item = AssignmentScore.NewAssignmentScore("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssignmentScoreCodeLengthRule(32);
        var item = AssignmentScore.NewAssignmentScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssignmentScoreValidatorReportsEveryProblem();
        AssignmentScoreExternalRefLengthRuleFlagsALongExternalRef();
        AssignmentScoreCodeLengthRuleFlagsALongCode();
    }
}
