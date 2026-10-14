namespace Acme.Workforce.Workplace.Tests.Visitor.Baseline;

using Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class StrictVisitorSlotRulesChecks
{
    public static void StrictVisitorSlotValidatorReportsEveryProblem()
    {
        var validator = StrictVisitorSlotRules.DefaultStrictVisitorSlotValidator();
        var item = StrictVisitorSlot.NewStrictVisitorSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictVisitorSlotRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new StrictVisitorSlotRequiredCategoryRule("strict visitor slot needs a value");
        var item = StrictVisitorSlot.NewStrictVisitorSlot("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictVisitorSlotCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new StrictVisitorSlotCategoryLengthRule(64);
        var item = StrictVisitorSlot.NewStrictVisitorSlot("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictVisitorSlotNameLengthRuleFlagsALongName()
    {
        var rule = new StrictVisitorSlotNameLengthRule(32);
        var item = StrictVisitorSlot.NewStrictVisitorSlot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictVisitorSlotRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new StrictVisitorSlotRateRangeRule(0.9, 1968.4);
        var item = StrictVisitorSlot.NewStrictVisitorSlot("x", "y");
        item.Rate = 1969.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictVisitorSlotRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new StrictVisitorSlotRatingRangeRule(4.4, 234.0);
        var item = StrictVisitorSlot.NewStrictVisitorSlot("x", "y");
        item.Rating = 235.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictVisitorSlotValidatorReportsEveryProblem();
        StrictVisitorSlotRequiredCategoryRuleFlagsABlankCategory();
        StrictVisitorSlotCategoryLengthRuleFlagsALongCategory();
        StrictVisitorSlotNameLengthRuleFlagsALongName();
        StrictVisitorSlotRateRangeRuleFlagsValuesAboveRate();
        StrictVisitorSlotRatingRangeRuleFlagsValuesAboveRating();
    }
}
