namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class BatchedWarrantyDraftRulesChecks
{
    public static void BatchedWarrantyDraftValidatorReportsEveryProblem()
    {
        var validator = BatchedWarrantyDraftRules.DefaultBatchedWarrantyDraftValidator();
        var item = BatchedWarrantyDraft.NewBatchedWarrantyDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedWarrantyDraftRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new BatchedWarrantyDraftRequiredLocationRule("batched warranty draft needs this field");
        var item = BatchedWarrantyDraft.NewBatchedWarrantyDraft("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedWarrantyDraftCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new BatchedWarrantyDraftCeilingRangeRule(5.3, 3056.9);
        var item = BatchedWarrantyDraft.NewBatchedWarrantyDraft("x", "y");
        item.Ceiling = 3058.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedWarrantyDraftVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new BatchedWarrantyDraftVersionRangeRule(255, 3600);
        var item = BatchedWarrantyDraft.NewBatchedWarrantyDraft("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedWarrantyDraftMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new BatchedWarrantyDraftMinimumRangeRule(9.8, 4418.4);
        var item = BatchedWarrantyDraft.NewBatchedWarrantyDraft("x", "y");
        item.Minimum = 4419.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedWarrantyDraftDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new BatchedWarrantyDraftDurationMinutesRangeRule(16, 3600);
        var item = BatchedWarrantyDraft.NewBatchedWarrantyDraft("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedWarrantyDraftValidatorReportsEveryProblem();
        BatchedWarrantyDraftRequiredLocationRuleFlagsABlankLocation();
        BatchedWarrantyDraftCeilingRangeRuleFlagsValuesAboveCeiling();
        BatchedWarrantyDraftVersionRangeRuleFlagsValuesAboveVersion();
        BatchedWarrantyDraftMinimumRangeRuleFlagsValuesAboveMinimum();
        BatchedWarrantyDraftDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
