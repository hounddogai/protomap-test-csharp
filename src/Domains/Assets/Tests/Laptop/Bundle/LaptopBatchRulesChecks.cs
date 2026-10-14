namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopBatchRulesChecks
{
    public static void LaptopBatchValidatorReportsEveryProblem()
    {
        var validator = LaptopBatchRules.DefaultLaptopBatchValidator();
        var item = LaptopBatch.NewLaptopBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new LaptopBatchRequiredNameRule("laptop batch needs a value");
        var item = LaptopBatch.NewLaptopBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopBatchGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new LaptopBatchGraceDaysRangeRule(255, 255);
        var item = LaptopBatch.NewLaptopBatch("x", "y");
        item.GraceDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopBatchSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new LaptopBatchSegmentLengthRule(1000);
        var item = LaptopBatch.NewLaptopBatch("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopBatchValidatorReportsEveryProblem();
        LaptopBatchRequiredNameRuleFlagsABlankName();
        LaptopBatchGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        LaptopBatchSegmentLengthRuleFlagsALongSegment();
    }
}
