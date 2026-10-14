namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LenientHolidayAdjustmentRulesChecks
{
    public static void LenientHolidayAdjustmentValidatorReportsEveryProblem()
    {
        var validator = LenientHolidayAdjustmentRules.DefaultLenientHolidayAdjustmentValidator();
        var item = LenientHolidayAdjustment.NewLenientHolidayAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientHolidayAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientHolidayAdjustmentRequiredCodeRule("lenient holiday adjustment needs this field");
        var item = LenientHolidayAdjustment.NewLenientHolidayAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientHolidayAdjustmentRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new LenientHolidayAdjustmentRequiredCategoryRule("lenient holiday adjustment needs a value");
        var item = LenientHolidayAdjustment.NewLenientHolidayAdjustment("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientHolidayAdjustmentRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new LenientHolidayAdjustmentRequiredNotesRule("lenient holiday adjustment needs this field");
        var item = LenientHolidayAdjustment.NewLenientHolidayAdjustment("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientHolidayAdjustmentScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new LenientHolidayAdjustmentScoreRangeRule(2.1, 318.7);
        var item = LenientHolidayAdjustment.NewLenientHolidayAdjustment("x", "y");
        item.Score = 320.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientHolidayAdjustmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientHolidayAdjustmentRequiredNameRule("lenient holiday adjustment needs this field");
        var item = LenientHolidayAdjustment.NewLenientHolidayAdjustment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientHolidayAdjustmentValidatorReportsEveryProblem();
        LenientHolidayAdjustmentRequiredCodeRuleFlagsABlankCode();
        LenientHolidayAdjustmentRequiredCategoryRuleFlagsABlankCategory();
        LenientHolidayAdjustmentRequiredNotesRuleFlagsABlankNotes();
        LenientHolidayAdjustmentScoreRangeRuleFlagsValuesAboveScore();
        LenientHolidayAdjustmentRequiredNameRuleFlagsABlankName();
    }
}
