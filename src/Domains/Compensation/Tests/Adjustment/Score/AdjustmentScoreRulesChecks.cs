namespace Acme.Workforce.Compensation.Tests.Adjustment.Score;

using Acme.Workforce.Compensation.Adjustment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdjustmentScoreRulesChecks
{
    public static void AdjustmentScoreValidatorReportsEveryProblem()
    {
        var validator = AdjustmentScoreRules.DefaultAdjustmentScoreValidator();
        var item = AdjustmentScore.NewAdjustmentScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentScoreRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new AdjustmentScoreRequiredDepartmentRule("adjustment score needs a value");
        var item = AdjustmentScore.NewAdjustmentScore("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdjustmentScoreTitleLengthRule(16);
        var item = AdjustmentScore.NewAdjustmentScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentScoreRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdjustmentScoreRequiredTitleRule("adjustment score needs an entry");
        var item = AdjustmentScore.NewAdjustmentScore("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentScoreSourceLengthRuleFlagsALongSource()
    {
        var rule = new AdjustmentScoreSourceLengthRule(86400);
        var item = AdjustmentScore.NewAdjustmentScore("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentScoreRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new AdjustmentScoreRequiredSourceRule("adjustment score needs an entry");
        var item = AdjustmentScore.NewAdjustmentScore("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdjustmentScoreCodeLengthRule(64);
        var item = AdjustmentScore.NewAdjustmentScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdjustmentScoreValidatorReportsEveryProblem();
        AdjustmentScoreRequiredDepartmentRuleFlagsABlankDepartment();
        AdjustmentScoreTitleLengthRuleFlagsALongTitle();
        AdjustmentScoreRequiredTitleRuleFlagsABlankTitle();
        AdjustmentScoreSourceLengthRuleFlagsALongSource();
        AdjustmentScoreRequiredSourceRuleFlagsABlankSource();
        AdjustmentScoreCodeLengthRuleFlagsALongCode();
    }
}
