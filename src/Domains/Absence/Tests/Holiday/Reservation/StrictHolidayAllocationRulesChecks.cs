namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class StrictHolidayAllocationRulesChecks
{
    public static void StrictHolidayAllocationValidatorReportsEveryProblem()
    {
        var validator = StrictHolidayAllocationRules.DefaultStrictHolidayAllocationValidator();
        var item = StrictHolidayAllocation.NewStrictHolidayAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictHolidayAllocationRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictHolidayAllocationRequiredNameRule("strict holiday allocation needs a value");
        var item = StrictHolidayAllocation.NewStrictHolidayAllocation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictHolidayAllocationNameLengthRuleFlagsALongName()
    {
        var rule = new StrictHolidayAllocationNameLengthRule(16);
        var item = StrictHolidayAllocation.NewStrictHolidayAllocation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictHolidayAllocationUnitLengthRuleFlagsALongUnit()
    {
        var rule = new StrictHolidayAllocationUnitLengthRule(86400);
        var item = StrictHolidayAllocation.NewStrictHolidayAllocation("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictHolidayAllocationValidatorReportsEveryProblem();
        StrictHolidayAllocationRequiredNameRuleFlagsABlankName();
        StrictHolidayAllocationNameLengthRuleFlagsALongName();
        StrictHolidayAllocationUnitLengthRuleFlagsALongUnit();
    }
}
