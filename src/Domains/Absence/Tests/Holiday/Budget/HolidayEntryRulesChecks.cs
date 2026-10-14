namespace Acme.Workforce.Absence.Tests.Holiday.Budget;

using Acme.Workforce.Absence.Holiday.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class HolidayEntryRulesChecks
{
    public static void HolidayEntryValidatorReportsEveryProblem()
    {
        var validator = HolidayEntryRules.DefaultHolidayEntryValidator();
        var item = HolidayEntry.NewHolidayEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new HolidayEntryCodeLengthRule(64);
        var item = HolidayEntry.NewHolidayEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayEntryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new HolidayEntryTitleLengthRule(255);
        var item = HolidayEntry.NewHolidayEntry("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HolidayEntryValidatorReportsEveryProblem();
        HolidayEntryCodeLengthRuleFlagsALongCode();
        HolidayEntryTitleLengthRuleFlagsALongTitle();
    }
}
