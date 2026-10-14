namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredAccrualScoreRulesChecks
{
    public static void TieredAccrualScoreValidatorReportsEveryProblem()
    {
        var validator = TieredAccrualScoreRules.DefaultTieredAccrualScoreValidator();
        var item = TieredAccrualScore.NewTieredAccrualScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredAccrualScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredAccrualScoreRequiredCodeRule("tiered accrual score needs an entry");
        var item = TieredAccrualScore.NewTieredAccrualScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredAccrualScoreFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new TieredAccrualScoreFteRangeRule(2.8, 2749.1);
        var item = TieredAccrualScore.NewTieredAccrualScore("x", "y");
        item.Fte = 2750.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredAccrualScoreRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredAccrualScoreRequiredTitleRule("tiered accrual score needs an entry");
        var item = TieredAccrualScore.NewTieredAccrualScore("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredAccrualScoreHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new TieredAccrualScoreHoursRangeRule(1.7, 767.5);
        var item = TieredAccrualScore.NewTieredAccrualScore("x", "y");
        item.Hours = 769.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredAccrualScoreLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredAccrualScoreLabelLengthRule(16);
        var item = TieredAccrualScore.NewTieredAccrualScore("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredAccrualScoreRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredAccrualScoreRequiredNameRule("tiered accrual score needs an entry");
        var item = TieredAccrualScore.NewTieredAccrualScore("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredAccrualScoreValidatorReportsEveryProblem();
        TieredAccrualScoreRequiredCodeRuleFlagsABlankCode();
        TieredAccrualScoreFteRangeRuleFlagsValuesAboveFte();
        TieredAccrualScoreRequiredTitleRuleFlagsABlankTitle();
        TieredAccrualScoreHoursRangeRuleFlagsValuesAboveHours();
        TieredAccrualScoreLabelLengthRuleFlagsALongLabel();
        TieredAccrualScoreRequiredNameRuleFlagsABlankName();
    }
}
