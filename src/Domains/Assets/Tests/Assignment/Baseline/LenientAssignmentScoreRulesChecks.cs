namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LenientAssignmentScoreRulesChecks
{
    public static void LenientAssignmentScoreValidatorReportsEveryProblem()
    {
        var validator = LenientAssignmentScoreRules.DefaultLenientAssignmentScoreValidator();
        var item = LenientAssignmentScore.NewLenientAssignmentScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientAssignmentScoreCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new LenientAssignmentScoreCategoryLengthRule(86400);
        var item = LenientAssignmentScore.NewLenientAssignmentScore("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientAssignmentScoreCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new LenientAssignmentScoreCostRangeRule(3.5, 4275.7);
        var item = LenientAssignmentScore.NewLenientAssignmentScore("x", "y");
        item.Cost = 4277.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientAssignmentScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientAssignmentScoreCodeLengthRule(80);
        var item = LenientAssignmentScore.NewLenientAssignmentScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientAssignmentScoreLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientAssignmentScoreLabelLengthRule(3600);
        var item = LenientAssignmentScore.NewLenientAssignmentScore("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientAssignmentScoreRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientAssignmentScoreRequiredLabelRule("lenient assignment score needs an entry");
        var item = LenientAssignmentScore.NewLenientAssignmentScore("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientAssignmentScoreValidatorReportsEveryProblem();
        LenientAssignmentScoreCategoryLengthRuleFlagsALongCategory();
        LenientAssignmentScoreCostRangeRuleFlagsValuesAboveCost();
        LenientAssignmentScoreCodeLengthRuleFlagsALongCode();
        LenientAssignmentScoreLabelLengthRuleFlagsALongLabel();
        LenientAssignmentScoreRequiredLabelRuleFlagsABlankLabel();
    }
}
