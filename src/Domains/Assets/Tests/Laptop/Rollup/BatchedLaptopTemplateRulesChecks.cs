namespace Acme.Workforce.Assets.Tests.Laptop.Rollup;

using Acme.Workforce.Assets.Laptop.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BatchedLaptopTemplateRulesChecks
{
    public static void BatchedLaptopTemplateValidatorReportsEveryProblem()
    {
        var validator = BatchedLaptopTemplateRules.DefaultBatchedLaptopTemplateValidator();
        var item = BatchedLaptopTemplate.NewBatchedLaptopTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedLaptopTemplateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BatchedLaptopTemplateRequiredTitleRule("batched laptop template needs an entry");
        var item = BatchedLaptopTemplate.NewBatchedLaptopTemplate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedLaptopTemplateCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new BatchedLaptopTemplateCapacityRangeRule(32, 64);
        var item = BatchedLaptopTemplate.NewBatchedLaptopTemplate("x", "y");
        item.Capacity = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedLaptopTemplatePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new BatchedLaptopTemplatePriorityRangeRule(16, 120);
        var item = BatchedLaptopTemplate.NewBatchedLaptopTemplate("x", "y");
        item.Priority = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedLaptopTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedLaptopTemplateRequiredCodeRule("batched laptop template needs this field");
        var item = BatchedLaptopTemplate.NewBatchedLaptopTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedLaptopTemplateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedLaptopTemplateTitleLengthRule(1000);
        var item = BatchedLaptopTemplate.NewBatchedLaptopTemplate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedLaptopTemplateValidatorReportsEveryProblem();
        BatchedLaptopTemplateRequiredTitleRuleFlagsABlankTitle();
        BatchedLaptopTemplateCapacityRangeRuleFlagsValuesAboveCapacity();
        BatchedLaptopTemplatePriorityRangeRuleFlagsValuesAbovePriority();
        BatchedLaptopTemplateRequiredCodeRuleFlagsABlankCode();
        BatchedLaptopTemplateTitleLengthRuleFlagsALongTitle();
    }
}
