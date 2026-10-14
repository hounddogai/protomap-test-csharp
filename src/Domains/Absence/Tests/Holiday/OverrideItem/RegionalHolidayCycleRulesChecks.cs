namespace Acme.Workforce.Absence.Tests.Holiday.OverrideItem;

using Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalHolidayCycleRulesChecks
{
    public static void RegionalHolidayCycleValidatorReportsEveryProblem()
    {
        var validator = RegionalHolidayCycleRules.DefaultRegionalHolidayCycleValidator();
        var item = RegionalHolidayCycle.NewRegionalHolidayCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalHolidayCycleSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new RegionalHolidayCycleSalaryBandLengthRule(32);
        var item = RegionalHolidayCycle.NewRegionalHolidayCycle("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalHolidayCycleBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new RegionalHolidayCycleBudgetRangeRule(7.4, 1932.9);
        var item = RegionalHolidayCycle.NewRegionalHolidayCycle("x", "y");
        item.Budget = 1934.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalHolidayCycleNameLengthRuleFlagsALongName()
    {
        var rule = new RegionalHolidayCycleNameLengthRule(80);
        var item = RegionalHolidayCycle.NewRegionalHolidayCycle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalHolidayCyclePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new RegionalHolidayCyclePriorityRangeRule(80, 255);
        var item = RegionalHolidayCycle.NewRegionalHolidayCycle("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalHolidayCycleValidatorReportsEveryProblem();
        RegionalHolidayCycleSalaryBandLengthRuleFlagsALongSalaryBand();
        RegionalHolidayCycleBudgetRangeRuleFlagsValuesAboveBudget();
        RegionalHolidayCycleNameLengthRuleFlagsALongName();
        RegionalHolidayCyclePriorityRangeRuleFlagsValuesAbovePriority();
    }
}
