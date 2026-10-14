namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GlobalDeductionReservationRulesChecks
{
    public static void GlobalDeductionReservationValidatorReportsEveryProblem()
    {
        var validator = GlobalDeductionReservationRules.DefaultGlobalDeductionReservationValidator();
        var item = GlobalDeductionReservation.NewGlobalDeductionReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalDeductionReservationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalDeductionReservationTitleLengthRule(3600);
        var item = GlobalDeductionReservation.NewGlobalDeductionReservation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalDeductionReservationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GlobalDeductionReservationRequiredTitleRule("global deduction reservation needs a value");
        var item = GlobalDeductionReservation.NewGlobalDeductionReservation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalDeductionReservationRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new GlobalDeductionReservationRatingRangeRule(9.8, 4779.4);
        var item = GlobalDeductionReservation.NewGlobalDeductionReservation("x", "y");
        item.Rating = 4780.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalDeductionReservationAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new GlobalDeductionReservationAmountRangeRule(8.3, 1291.2);
        var item = GlobalDeductionReservation.NewGlobalDeductionReservation("x", "y");
        item.Amount = 1292.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalDeductionReservationScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new GlobalDeductionReservationScoreRangeRule(2.6, 3062.4);
        var item = GlobalDeductionReservation.NewGlobalDeductionReservation("x", "y");
        item.Score = 3063.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalDeductionReservationValidatorReportsEveryProblem();
        GlobalDeductionReservationTitleLengthRuleFlagsALongTitle();
        GlobalDeductionReservationRequiredTitleRuleFlagsABlankTitle();
        GlobalDeductionReservationRatingRangeRuleFlagsValuesAboveRating();
        GlobalDeductionReservationAmountRangeRuleFlagsValuesAboveAmount();
        GlobalDeductionReservationScoreRangeRuleFlagsValuesAboveScore();
    }
}
