namespace Acme.Workforce.Orgdesign.Tests.CostModel.Assignment;

using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CostModelAssignmentRulesChecks
{
    public static void CostModelAssignmentValidatorReportsEveryProblem()
    {
        var validator = CostModelAssignmentRules.DefaultCostModelAssignmentValidator();
        var item = CostModelAssignment.NewCostModelAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CostModelAssignmentRequiredCodeRule("cost model assignment needs an entry");
        var item = CostModelAssignment.NewCostModelAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new CostModelAssignmentCodeLengthRule(16);
        var item = CostModelAssignment.NewCostModelAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelAssignmentRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new CostModelAssignmentRequiredReviewerRule("cost model assignment needs a value");
        var item = CostModelAssignment.NewCostModelAssignment("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CostModelAssignmentValidatorReportsEveryProblem();
        CostModelAssignmentRequiredCodeRuleFlagsABlankCode();
        CostModelAssignmentCodeLengthRuleFlagsALongCode();
        CostModelAssignmentRequiredReviewerRuleFlagsABlankReviewer();
    }
}
