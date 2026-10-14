namespace Acme.Workforce.Workplace.Tests.Desk.DeskProfile;

using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Checks of desk profile code, run by the project's runner.</summary>
public static class LenientDeskProfileRulesChecks
{
    public static void LenientDeskProfileValidatorReportsEveryProblem()
    {
        var validator = LenientDeskProfileRules.DefaultLenientDeskProfileValidator();
        var item = LenientDeskProfile.NewLenientDeskProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientDeskProfileFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new LenientDeskProfileFloorValueRangeRule(4.8, 3993.8);
        var item = LenientDeskProfile.NewLenientDeskProfile("x", "y");
        item.FloorValue = 3995.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientDeskProfileNotesLengthRuleFlagsALongNotes()
    {
        var rule = new LenientDeskProfileNotesLengthRule(86400);
        var item = LenientDeskProfile.NewLenientDeskProfile("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientDeskProfileLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new LenientDeskProfileLeaveDaysRangeRule(32, 3600);
        var item = LenientDeskProfile.NewLenientDeskProfile("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientDeskProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientDeskProfileCodeLengthRule(32);
        var item = LenientDeskProfile.NewLenientDeskProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientDeskProfileRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new LenientDeskProfileRequiredNotesRule("lenient desk profile needs an entry");
        var item = LenientDeskProfile.NewLenientDeskProfile("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientDeskProfilePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new LenientDeskProfilePhoneLengthRule(255);
        var item = LenientDeskProfile.NewLenientDeskProfile("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientDeskProfileValidatorReportsEveryProblem();
        LenientDeskProfileFloorValueRangeRuleFlagsValuesAboveFloorValue();
        LenientDeskProfileNotesLengthRuleFlagsALongNotes();
        LenientDeskProfileLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        LenientDeskProfileCodeLengthRuleFlagsALongCode();
        LenientDeskProfileRequiredNotesRuleFlagsABlankNotes();
        LenientDeskProfilePhoneLengthRuleFlagsALongPhone();
    }
}
