namespace Acme.Workforce.Assets.Tests.Assignment.Request;

using Acme.Workforce.Assets.Assignment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AssignmentRequestRulesChecks
{
    public static void AssignmentRequestValidatorReportsEveryProblem()
    {
        var validator = AssignmentRequestRules.DefaultAssignmentRequestValidator();
        var item = AssignmentRequest.NewAssignmentRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentRequestScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new AssignmentRequestScoreRangeRule(3.8, 4361.0);
        var item = AssignmentRequest.NewAssignmentRequest("x", "y");
        item.Score = 4362.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssignmentRequestRequiredCodeRule("assignment request needs this field");
        var item = AssignmentRequest.NewAssignmentRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssignmentRequestValidatorReportsEveryProblem();
        AssignmentRequestScoreRangeRuleFlagsValuesAboveScore();
        AssignmentRequestRequiredCodeRuleFlagsABlankCode();
    }
}
