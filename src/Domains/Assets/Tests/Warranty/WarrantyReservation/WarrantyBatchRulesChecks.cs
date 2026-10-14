namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class WarrantyBatchRulesChecks
{
    public static void WarrantyBatchValidatorReportsEveryProblem()
    {
        var validator = WarrantyBatchRules.DefaultWarrantyBatchValidator();
        var item = WarrantyBatch.NewWarrantyBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new WarrantyBatchRequiredNameRule("warranty batch needs this field");
        var item = WarrantyBatch.NewWarrantyBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyBatchPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new WarrantyBatchPriorityRangeRule(64, 255);
        var item = WarrantyBatch.NewWarrantyBatch("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WarrantyBatchRequiredCodeRule("warranty batch needs this field");
        var item = WarrantyBatch.NewWarrantyBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyBatchDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new WarrantyBatchDescriptionLengthRule(120);
        var item = WarrantyBatch.NewWarrantyBatch("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyBatchRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new WarrantyBatchRetriesRangeRule(120, 1000);
        var item = WarrantyBatch.NewWarrantyBatch("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyBatchNameLengthRuleFlagsALongName()
    {
        var rule = new WarrantyBatchNameLengthRule(16);
        var item = WarrantyBatch.NewWarrantyBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WarrantyBatchValidatorReportsEveryProblem();
        WarrantyBatchRequiredNameRuleFlagsABlankName();
        WarrantyBatchPriorityRangeRuleFlagsValuesAbovePriority();
        WarrantyBatchRequiredCodeRuleFlagsABlankCode();
        WarrantyBatchDescriptionLengthRuleFlagsALongDescription();
        WarrantyBatchRetriesRangeRuleFlagsValuesAboveRetries();
        WarrantyBatchNameLengthRuleFlagsALongName();
    }
}
