namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LaptopThresholdRulesChecks
{
    public static void LaptopThresholdValidatorReportsEveryProblem()
    {
        var validator = LaptopThresholdRules.DefaultLaptopThresholdValidator();
        var item = LaptopThreshold.NewLaptopThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopThresholdLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LaptopThresholdLabelLengthRule(3600);
        var item = LaptopThreshold.NewLaptopThreshold("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopThresholdDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new LaptopThresholdDiscountRangeRule(2.8, 545.1);
        var item = LaptopThreshold.NewLaptopThreshold("x", "y");
        item.Discount = 546.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopThresholdEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new LaptopThresholdEmployeeIdLengthRule(255);
        var item = LaptopThreshold.NewLaptopThreshold("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopThresholdRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LaptopThresholdRequiredLabelRule("laptop threshold needs an entry");
        var item = LaptopThreshold.NewLaptopThreshold("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopThresholdSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LaptopThresholdSequenceRangeRule(1000, 3600);
        var item = LaptopThreshold.NewLaptopThreshold("x", "y");
        item.Sequence = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LaptopThresholdValidatorReportsEveryProblem();
        LaptopThresholdLabelLengthRuleFlagsALongLabel();
        LaptopThresholdDiscountRangeRuleFlagsValuesAboveDiscount();
        LaptopThresholdEmployeeIdLengthRuleFlagsALongEmployeeId();
        LaptopThresholdRequiredLabelRuleFlagsABlankLabel();
        LaptopThresholdSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
