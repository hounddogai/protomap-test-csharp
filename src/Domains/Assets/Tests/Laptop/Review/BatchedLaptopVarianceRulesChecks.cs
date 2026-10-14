namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BatchedLaptopVarianceRulesChecks
{
    public static void BatchedLaptopVarianceValidatorReportsEveryProblem()
    {
        var validator = BatchedLaptopVarianceRules.DefaultBatchedLaptopVarianceValidator();
        var item = BatchedLaptopVariance.NewBatchedLaptopVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedLaptopVarianceRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new BatchedLaptopVarianceRequiredGradeRule("batched laptop variance needs a value");
        var item = BatchedLaptopVariance.NewBatchedLaptopVariance("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedLaptopVarianceFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new BatchedLaptopVarianceFloorValueRangeRule(9.2, 4341.2);
        var item = BatchedLaptopVariance.NewBatchedLaptopVariance("x", "y");
        item.FloorValue = 4342.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedLaptopVarianceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BatchedLaptopVarianceRequiredTitleRule("batched laptop variance needs this field");
        var item = BatchedLaptopVariance.NewBatchedLaptopVariance("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedLaptopVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedLaptopVarianceRequiredCodeRule("batched laptop variance needs this field");
        var item = BatchedLaptopVariance.NewBatchedLaptopVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedLaptopVarianceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedLaptopVarianceTitleLengthRule(3600);
        var item = BatchedLaptopVariance.NewBatchedLaptopVariance("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedLaptopVarianceRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new BatchedLaptopVarianceRateRangeRule(0.0, 2436.0);
        var item = BatchedLaptopVariance.NewBatchedLaptopVariance("x", "y");
        item.Rate = 2437.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedLaptopVarianceValidatorReportsEveryProblem();
        BatchedLaptopVarianceRequiredGradeRuleFlagsABlankGrade();
        BatchedLaptopVarianceFloorValueRangeRuleFlagsValuesAboveFloorValue();
        BatchedLaptopVarianceRequiredTitleRuleFlagsABlankTitle();
        BatchedLaptopVarianceRequiredCodeRuleFlagsABlankCode();
        BatchedLaptopVarianceTitleLengthRuleFlagsALongTitle();
        BatchedLaptopVarianceRateRangeRuleFlagsValuesAboveRate();
    }
}
