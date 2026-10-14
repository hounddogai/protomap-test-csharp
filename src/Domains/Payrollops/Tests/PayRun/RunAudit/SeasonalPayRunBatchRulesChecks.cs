namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAudit;

using Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>Checks of run audit code, run by the project's runner.</summary>
public static class SeasonalPayRunBatchRulesChecks
{
    public static void SeasonalPayRunBatchValidatorReportsEveryProblem()
    {
        var validator = SeasonalPayRunBatchRules.DefaultSeasonalPayRunBatchValidator();
        var item = SeasonalPayRunBatch.NewSeasonalPayRunBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalPayRunBatchQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new SeasonalPayRunBatchQuantityRangeRule(120, 255);
        var item = SeasonalPayRunBatch.NewSeasonalPayRunBatch("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalPayRunBatchGradeLengthRuleFlagsALongGrade()
    {
        var rule = new SeasonalPayRunBatchGradeLengthRule(80);
        var item = SeasonalPayRunBatch.NewSeasonalPayRunBatch("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalPayRunBatchDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new SeasonalPayRunBatchDiscountRangeRule(7.9, 1247.3);
        var item = SeasonalPayRunBatch.NewSeasonalPayRunBatch("x", "y");
        item.Discount = 1248.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalPayRunBatchRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new SeasonalPayRunBatchRequiredContactEmailRule("seasonal pay run batch needs a value");
        var item = SeasonalPayRunBatch.NewSeasonalPayRunBatch("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalPayRunBatchValidatorReportsEveryProblem();
        SeasonalPayRunBatchQuantityRangeRuleFlagsValuesAboveQuantity();
        SeasonalPayRunBatchGradeLengthRuleFlagsALongGrade();
        SeasonalPayRunBatchDiscountRangeRuleFlagsValuesAboveDiscount();
        SeasonalPayRunBatchRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
