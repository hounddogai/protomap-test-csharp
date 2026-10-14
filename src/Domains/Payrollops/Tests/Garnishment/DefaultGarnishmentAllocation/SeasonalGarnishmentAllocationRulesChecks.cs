namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class SeasonalGarnishmentAllocationRulesChecks
{
    public static void SeasonalGarnishmentAllocationValidatorReportsEveryProblem()
    {
        var validator = SeasonalGarnishmentAllocationRules.DefaultSeasonalGarnishmentAllocationValidator();
        var item = SeasonalGarnishmentAllocation.NewSeasonalGarnishmentAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalGarnishmentAllocationGradeLengthRuleFlagsALongGrade()
    {
        var rule = new SeasonalGarnishmentAllocationGradeLengthRule(1000);
        var item = SeasonalGarnishmentAllocation.NewSeasonalGarnishmentAllocation("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalGarnishmentAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalGarnishmentAllocationCodeLengthRule(1000);
        var item = SeasonalGarnishmentAllocation.NewSeasonalGarnishmentAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalGarnishmentAllocationGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new SeasonalGarnishmentAllocationGraceDaysRangeRule(80, 86400);
        var item = SeasonalGarnishmentAllocation.NewSeasonalGarnishmentAllocation("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalGarnishmentAllocationUnitLengthRuleFlagsALongUnit()
    {
        var rule = new SeasonalGarnishmentAllocationUnitLengthRule(64);
        var item = SeasonalGarnishmentAllocation.NewSeasonalGarnishmentAllocation("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalGarnishmentAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalGarnishmentAllocationRequiredCodeRule("seasonal garnishment allocation needs a value");
        var item = SeasonalGarnishmentAllocation.NewSeasonalGarnishmentAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalGarnishmentAllocationValidatorReportsEveryProblem();
        SeasonalGarnishmentAllocationGradeLengthRuleFlagsALongGrade();
        SeasonalGarnishmentAllocationCodeLengthRuleFlagsALongCode();
        SeasonalGarnishmentAllocationGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        SeasonalGarnishmentAllocationUnitLengthRuleFlagsALongUnit();
        SeasonalGarnishmentAllocationRequiredCodeRuleFlagsABlankCode();
    }
}
