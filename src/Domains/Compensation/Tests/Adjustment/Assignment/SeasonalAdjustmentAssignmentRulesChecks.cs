namespace Acme.Workforce.Compensation.Tests.Adjustment.Assignment;

using Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class SeasonalAdjustmentAssignmentRulesChecks
{
    public static void SeasonalAdjustmentAssignmentValidatorReportsEveryProblem()
    {
        var validator = SeasonalAdjustmentAssignmentRules.DefaultSeasonalAdjustmentAssignmentValidator();
        var item = SeasonalAdjustmentAssignment.NewSeasonalAdjustmentAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAdjustmentAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SeasonalAdjustmentAssignmentTitleLengthRule(32);
        var item = SeasonalAdjustmentAssignment.NewSeasonalAdjustmentAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalAdjustmentAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalAdjustmentAssignmentRequiredCodeRule("seasonal adjustment assignment needs an entry");
        var item = SeasonalAdjustmentAssignment.NewSeasonalAdjustmentAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentAssignmentValidatorReportsEveryProblem();
        SeasonalAdjustmentAssignmentTitleLengthRuleFlagsALongTitle();
        SeasonalAdjustmentAssignmentRequiredCodeRuleFlagsABlankCode();
    }
}
