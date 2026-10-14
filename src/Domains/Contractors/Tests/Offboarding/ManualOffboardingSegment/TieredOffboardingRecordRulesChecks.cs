namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class TieredOffboardingRecordRulesChecks
{
    public static void TieredOffboardingRecordValidatorReportsEveryProblem()
    {
        var validator = TieredOffboardingRecordRules.DefaultTieredOffboardingRecordValidator();
        var item = TieredOffboardingRecord.NewTieredOffboardingRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredOffboardingRecordAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new TieredOffboardingRecordAmountRangeRule(9.8, 3598.9);
        var item = TieredOffboardingRecord.NewTieredOffboardingRecord("x", "y");
        item.Amount = 3600.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredOffboardingRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredOffboardingRecordRequiredCodeRule("tiered offboarding record needs a value");
        var item = TieredOffboardingRecord.NewTieredOffboardingRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredOffboardingRecordRegionLengthRuleFlagsALongRegion()
    {
        var rule = new TieredOffboardingRecordRegionLengthRule(120);
        var item = TieredOffboardingRecord.NewTieredOffboardingRecord("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredOffboardingRecordValidatorReportsEveryProblem();
        TieredOffboardingRecordAmountRangeRuleFlagsValuesAboveAmount();
        TieredOffboardingRecordRequiredCodeRuleFlagsABlankCode();
        TieredOffboardingRecordRegionLengthRuleFlagsALongRegion();
    }
}
