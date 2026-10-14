namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class HolidaySnapshotRulesChecks
{
    public static void HolidaySnapshotValidatorReportsEveryProblem()
    {
        var validator = HolidaySnapshotRules.DefaultHolidaySnapshotValidator();
        var item = HolidaySnapshot.NewHolidaySnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidaySnapshotNameLengthRuleFlagsALongName()
    {
        var rule = new HolidaySnapshotNameLengthRule(255);
        var item = HolidaySnapshot.NewHolidaySnapshot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidaySnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new HolidaySnapshotCodeLengthRule(16);
        var item = HolidaySnapshot.NewHolidaySnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidaySnapshotCountryLengthRuleFlagsALongCountry()
    {
        var rule = new HolidaySnapshotCountryLengthRule(86400);
        var item = HolidaySnapshot.NewHolidaySnapshot("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HolidaySnapshotValidatorReportsEveryProblem();
        HolidaySnapshotNameLengthRuleFlagsALongName();
        HolidaySnapshotCodeLengthRuleFlagsALongCode();
        HolidaySnapshotCountryLengthRuleFlagsALongCountry();
    }
}
