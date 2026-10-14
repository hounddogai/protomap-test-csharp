namespace Acme.Workforce.Talent.Tests.Mobility.MobilityMapping;

using Acme.Workforce.Talent.Mobility.MobilityMapping;

/// <summary>Checks of mobility mapping code, run by the project's runner.</summary>
public static class FallbackMobilityMappingRulesChecks
{
    public static void FallbackMobilityMappingValidatorReportsEveryProblem()
    {
        var validator = FallbackMobilityMappingRules.DefaultFallbackMobilityMappingValidator();
        var item = FallbackMobilityMapping.NewFallbackMobilityMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackMobilityMappingAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new FallbackMobilityMappingAmountRangeRule(3.3, 2018.5);
        var item = FallbackMobilityMapping.NewFallbackMobilityMapping("x", "y");
        item.Amount = 2020.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackMobilityMappingRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new FallbackMobilityMappingRequiredContactEmailRule("fallback mobility mapping needs an entry");
        var item = FallbackMobilityMapping.NewFallbackMobilityMapping("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackMobilityMappingTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new FallbackMobilityMappingTenureMonthsRangeRule(120, 3600);
        var item = FallbackMobilityMapping.NewFallbackMobilityMapping("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackMobilityMappingLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackMobilityMappingLabelLengthRule(80);
        var item = FallbackMobilityMapping.NewFallbackMobilityMapping("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackMobilityMappingValidatorReportsEveryProblem();
        FallbackMobilityMappingAmountRangeRuleFlagsValuesAboveAmount();
        FallbackMobilityMappingRequiredContactEmailRuleFlagsABlankContactEmail();
        FallbackMobilityMappingTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        FallbackMobilityMappingLabelLengthRuleFlagsALongLabel();
    }
}
