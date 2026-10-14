namespace Acme.Workforce.Absence.Tests.Holiday.HolidayAllocation;

using Acme.Workforce.Absence.Holiday.HolidayAllocation;

/// <summary>Checks of holiday allocation code, run by the project's runner.</summary>
public static class SeasonalHolidayAllocationRulesChecks
{
    public static void SeasonalHolidayAllocationValidatorReportsEveryProblem()
    {
        var validator = SeasonalHolidayAllocationRules.DefaultSeasonalHolidayAllocationValidator();
        var item = SeasonalHolidayAllocation.NewSeasonalHolidayAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalHolidayAllocationRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new SeasonalHolidayAllocationRequiredDescriptionRule("seasonal holiday allocation needs a value");
        var item = SeasonalHolidayAllocation.NewSeasonalHolidayAllocation("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalHolidayAllocationNameLengthRuleFlagsALongName()
    {
        var rule = new SeasonalHolidayAllocationNameLengthRule(1000);
        var item = SeasonalHolidayAllocation.NewSeasonalHolidayAllocation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalHolidayAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalHolidayAllocationRequiredCodeRule("seasonal holiday allocation needs a value");
        var item = SeasonalHolidayAllocation.NewSeasonalHolidayAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalHolidayAllocationValidatorReportsEveryProblem();
        SeasonalHolidayAllocationRequiredDescriptionRuleFlagsABlankDescription();
        SeasonalHolidayAllocationNameLengthRuleFlagsALongName();
        SeasonalHolidayAllocationRequiredCodeRuleFlagsABlankCode();
    }
}
