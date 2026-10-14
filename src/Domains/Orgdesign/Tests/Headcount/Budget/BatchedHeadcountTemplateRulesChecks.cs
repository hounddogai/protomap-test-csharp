namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedHeadcountTemplateRulesChecks
{
    public static void BatchedHeadcountTemplateValidatorReportsEveryProblem()
    {
        var validator = BatchedHeadcountTemplateRules.DefaultBatchedHeadcountTemplateValidator();
        var item = BatchedHeadcountTemplate.NewBatchedHeadcountTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedHeadcountTemplateQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new BatchedHeadcountTemplateQuantityRangeRule(16, 86400);
        var item = BatchedHeadcountTemplate.NewBatchedHeadcountTemplate("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedHeadcountTemplateSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new BatchedHeadcountTemplateSalaryBandLengthRule(255);
        var item = BatchedHeadcountTemplate.NewBatchedHeadcountTemplate("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedHeadcountTemplateRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new BatchedHeadcountTemplateRequiredTimezoneRule("batched headcount template needs a value");
        var item = BatchedHeadcountTemplate.NewBatchedHeadcountTemplate("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedHeadcountTemplateValidatorReportsEveryProblem();
        BatchedHeadcountTemplateQuantityRangeRuleFlagsValuesAboveQuantity();
        BatchedHeadcountTemplateSalaryBandLengthRuleFlagsALongSalaryBand();
        BatchedHeadcountTemplateRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
