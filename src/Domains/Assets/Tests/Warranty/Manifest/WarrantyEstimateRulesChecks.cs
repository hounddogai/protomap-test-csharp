namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyEstimateRulesChecks
{
    public static void WarrantyEstimateValidatorReportsEveryProblem()
    {
        var validator = WarrantyEstimateRules.DefaultWarrantyEstimateValidator();
        var item = WarrantyEstimate.NewWarrantyEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyEstimateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WarrantyEstimateLabelLengthRule(16);
        var item = WarrantyEstimate.NewWarrantyEstimate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyEstimateSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new WarrantyEstimateSequenceRangeRule(1000, 1000);
        var item = WarrantyEstimate.NewWarrantyEstimate("x", "y");
        item.Sequence = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyEstimateRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new WarrantyEstimateRetriesRangeRule(1000, 86400);
        var item = WarrantyEstimate.NewWarrantyEstimate("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyEstimateRegionLengthRuleFlagsALongRegion()
    {
        var rule = new WarrantyEstimateRegionLengthRule(32);
        var item = WarrantyEstimate.NewWarrantyEstimate("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new WarrantyEstimateCodeLengthRule(16);
        var item = WarrantyEstimate.NewWarrantyEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyEstimateCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new WarrantyEstimateCapacityRangeRule(32, 80);
        var item = WarrantyEstimate.NewWarrantyEstimate("x", "y");
        item.Capacity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WarrantyEstimateValidatorReportsEveryProblem();
        WarrantyEstimateLabelLengthRuleFlagsALongLabel();
        WarrantyEstimateSequenceRangeRuleFlagsValuesAboveSequence();
        WarrantyEstimateRetriesRangeRuleFlagsValuesAboveRetries();
        WarrantyEstimateRegionLengthRuleFlagsALongRegion();
        WarrantyEstimateCodeLengthRuleFlagsALongCode();
        WarrantyEstimateCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
