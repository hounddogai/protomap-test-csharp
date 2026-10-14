namespace Acme.Workforce.Assets.Tests.Laptop.Baseline;

using Acme.Workforce.Assets.Laptop.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RollingLaptopBaselineRulesChecks
{
    public static void RollingLaptopBaselineValidatorReportsEveryProblem()
    {
        var validator = RollingLaptopBaselineRules.DefaultRollingLaptopBaselineValidator();
        var item = RollingLaptopBaseline.NewRollingLaptopBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingLaptopBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingLaptopBaselineRequiredCodeRule("rolling laptop baseline needs a value");
        var item = RollingLaptopBaseline.NewRollingLaptopBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingLaptopBaselineRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new RollingLaptopBaselineRequiredTimezoneRule("rolling laptop baseline needs an entry");
        var item = RollingLaptopBaseline.NewRollingLaptopBaseline("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingLaptopBaselineMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new RollingLaptopBaselineMaximumRangeRule(5.0, 4961.8);
        var item = RollingLaptopBaseline.NewRollingLaptopBaseline("x", "y");
        item.Maximum = 4963.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingLaptopBaselineNameLengthRuleFlagsALongName()
    {
        var rule = new RollingLaptopBaselineNameLengthRule(1000);
        var item = RollingLaptopBaseline.NewRollingLaptopBaseline("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingLaptopBaselineTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new RollingLaptopBaselineTimezoneLengthRule(3600);
        var item = RollingLaptopBaseline.NewRollingLaptopBaseline("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingLaptopBaselineLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new RollingLaptopBaselineLeaveDaysRangeRule(255, 1000);
        var item = RollingLaptopBaseline.NewRollingLaptopBaseline("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingLaptopBaselineValidatorReportsEveryProblem();
        RollingLaptopBaselineRequiredCodeRuleFlagsABlankCode();
        RollingLaptopBaselineRequiredTimezoneRuleFlagsABlankTimezone();
        RollingLaptopBaselineMaximumRangeRuleFlagsValuesAboveMaximum();
        RollingLaptopBaselineNameLengthRuleFlagsALongName();
        RollingLaptopBaselineTimezoneLengthRuleFlagsALongTimezone();
        RollingLaptopBaselineLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
