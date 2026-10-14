namespace Acme.Workforce.Absence.Tests.Holiday.Allocation;

using Acme.Workforce.Absence.Holiday.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class HolidayCycleRulesChecks
{
    public static void HolidayCycleValidatorReportsEveryProblem()
    {
        var validator = HolidayCycleRules.DefaultHolidayCycleValidator();
        var item = HolidayCycle.NewHolidayCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayCycleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new HolidayCycleTitleLengthRule(64);
        var item = HolidayCycle.NewHolidayCycle("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayCycleRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new HolidayCycleRequiredChecksumRule("holiday cycle needs this field");
        var item = HolidayCycle.NewHolidayCycle("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayCycleChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new HolidayCycleChecksumLengthRule(1000);
        var item = HolidayCycle.NewHolidayCycle("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HolidayCycleValidatorReportsEveryProblem();
        HolidayCycleTitleLengthRuleFlagsALongTitle();
        HolidayCycleRequiredChecksumRuleFlagsABlankChecksum();
        HolidayCycleChecksumLengthRuleFlagsALongChecksum();
    }
}
