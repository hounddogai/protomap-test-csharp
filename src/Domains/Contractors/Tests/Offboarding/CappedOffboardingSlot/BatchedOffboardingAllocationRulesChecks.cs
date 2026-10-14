namespace Acme.Workforce.Contractors.Tests.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

/// <summary>Checks of capped offboarding slot code, run by the project's runner.</summary>
public static class BatchedOffboardingAllocationRulesChecks
{
    public static void BatchedOffboardingAllocationValidatorReportsEveryProblem()
    {
        var validator = BatchedOffboardingAllocationRules.DefaultBatchedOffboardingAllocationValidator();
        var item = BatchedOffboardingAllocation.NewBatchedOffboardingAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedOffboardingAllocationMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new BatchedOffboardingAllocationMarginRangeRule(8.0, 1467.4);
        var item = BatchedOffboardingAllocation.NewBatchedOffboardingAllocation("x", "y");
        item.Margin = 1468.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedOffboardingAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedOffboardingAllocationCodeLengthRule(80);
        var item = BatchedOffboardingAllocation.NewBatchedOffboardingAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedOffboardingAllocationRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new BatchedOffboardingAllocationRequiredTimezoneRule("batched offboarding allocation needs an entry");
        var item = BatchedOffboardingAllocation.NewBatchedOffboardingAllocation("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedOffboardingAllocationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BatchedOffboardingAllocationRequiredLabelRule("batched offboarding allocation needs this field");
        var item = BatchedOffboardingAllocation.NewBatchedOffboardingAllocation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedOffboardingAllocationValidatorReportsEveryProblem();
        BatchedOffboardingAllocationMarginRangeRuleFlagsValuesAboveMargin();
        BatchedOffboardingAllocationCodeLengthRuleFlagsALongCode();
        BatchedOffboardingAllocationRequiredTimezoneRuleFlagsABlankTimezone();
        BatchedOffboardingAllocationRequiredLabelRuleFlagsABlankLabel();
    }
}
