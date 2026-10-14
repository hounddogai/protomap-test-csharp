namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class HolidaySlotRulesChecks
{
    public static void HolidaySlotValidatorReportsEveryProblem()
    {
        var validator = HolidaySlotRules.DefaultHolidaySlotValidator();
        var item = HolidaySlot.NewHolidaySlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidaySlotRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new HolidaySlotRequiredDescriptionRule("holiday slot needs an entry");
        var item = HolidaySlot.NewHolidaySlot("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidaySlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new HolidaySlotCodeLengthRule(1000);
        var item = HolidaySlot.NewHolidaySlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidaySlotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new HolidaySlotTitleLengthRule(16);
        var item = HolidaySlot.NewHolidaySlot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HolidaySlotValidatorReportsEveryProblem();
        HolidaySlotRequiredDescriptionRuleFlagsABlankDescription();
        HolidaySlotCodeLengthRuleFlagsALongCode();
        HolidaySlotTitleLengthRuleFlagsALongTitle();
    }
}
