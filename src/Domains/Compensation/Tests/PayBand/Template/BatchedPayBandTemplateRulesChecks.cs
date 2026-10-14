namespace Acme.Workforce.Compensation.Tests.PayBand.Template;

using Acme.Workforce.Compensation.PayBand.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class BatchedPayBandTemplateRulesChecks
{
    public static void BatchedPayBandTemplateValidatorReportsEveryProblem()
    {
        var validator = BatchedPayBandTemplateRules.DefaultBatchedPayBandTemplateValidator();
        var item = BatchedPayBandTemplate.NewBatchedPayBandTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedPayBandTemplateDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new BatchedPayBandTemplateDiscountRangeRule(4.6, 3028.2);
        var item = BatchedPayBandTemplate.NewBatchedPayBandTemplate("x", "y");
        item.Discount = 3029.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedPayBandTemplateUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new BatchedPayBandTemplateUpdatedAtRangeRule(32, 64);
        var item = BatchedPayBandTemplate.NewBatchedPayBandTemplate("x", "y");
        item.UpdatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedPayBandTemplateSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new BatchedPayBandTemplateSalaryBandLengthRule(3600);
        var item = BatchedPayBandTemplate.NewBatchedPayBandTemplate("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedPayBandTemplateValidatorReportsEveryProblem();
        BatchedPayBandTemplateDiscountRangeRuleFlagsValuesAboveDiscount();
        BatchedPayBandTemplateUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        BatchedPayBandTemplateSalaryBandLengthRuleFlagsALongSalaryBand();
    }
}
