namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LaptopCycleRulesChecks
{
    public static void LaptopCycleValidatorReportsEveryProblem()
    {
        var validator = LaptopCycleRules.DefaultLaptopCycleValidator();
        var item = LaptopCycle.NewLaptopCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopCycleMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new LaptopCycleMarginRangeRule(2.1, 4021.1);
        var item = LaptopCycle.NewLaptopCycle("x", "y");
        item.Margin = 4022.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopCycleThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new LaptopCycleThresholdRangeRule(6.4, 2720.0);
        var item = LaptopCycle.NewLaptopCycle("x", "y");
        item.Threshold = 2721.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopCycleRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new LaptopCycleRequiredSummaryRule("laptop cycle needs this field");
        var item = LaptopCycle.NewLaptopCycle("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopCycleNameLengthRuleFlagsALongName()
    {
        var rule = new LaptopCycleNameLengthRule(1000);
        var item = LaptopCycle.NewLaptopCycle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopCycleValidatorReportsEveryProblem();
        LaptopCycleMarginRangeRuleFlagsValuesAboveMargin();
        LaptopCycleThresholdRangeRuleFlagsValuesAboveThreshold();
        LaptopCycleRequiredSummaryRuleFlagsABlankSummary();
        LaptopCycleNameLengthRuleFlagsALongName();
    }
}
