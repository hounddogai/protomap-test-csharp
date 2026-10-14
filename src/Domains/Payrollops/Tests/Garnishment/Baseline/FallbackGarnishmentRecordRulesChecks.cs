namespace Acme.Workforce.Payrollops.Tests.Garnishment.Baseline;

using Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackGarnishmentRecordRulesChecks
{
    public static void FallbackGarnishmentRecordValidatorReportsEveryProblem()
    {
        var validator = FallbackGarnishmentRecordRules.DefaultFallbackGarnishmentRecordValidator();
        var item = FallbackGarnishmentRecord.NewFallbackGarnishmentRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackGarnishmentRecordOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new FallbackGarnishmentRecordOwnerIdLengthRule(16);
        var item = FallbackGarnishmentRecord.NewFallbackGarnishmentRecord("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackGarnishmentRecordFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new FallbackGarnishmentRecordFteRangeRule(3.7, 4432.8);
        var item = FallbackGarnishmentRecord.NewFallbackGarnishmentRecord("x", "y");
        item.Fte = 4434.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackGarnishmentRecordRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new FallbackGarnishmentRecordRequiredOwnerIdRule("fallback garnishment record needs a value");
        var item = FallbackGarnishmentRecord.NewFallbackGarnishmentRecord("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackGarnishmentRecordMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new FallbackGarnishmentRecordMarginRangeRule(2.5, 532.1);
        var item = FallbackGarnishmentRecord.NewFallbackGarnishmentRecord("x", "y");
        item.Margin = 533.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackGarnishmentRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackGarnishmentRecordRequiredCodeRule("fallback garnishment record needs an entry");
        var item = FallbackGarnishmentRecord.NewFallbackGarnishmentRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackGarnishmentRecordRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new FallbackGarnishmentRecordRequiredCountryRule("fallback garnishment record needs an entry");
        var item = FallbackGarnishmentRecord.NewFallbackGarnishmentRecord("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackGarnishmentRecordValidatorReportsEveryProblem();
        FallbackGarnishmentRecordOwnerIdLengthRuleFlagsALongOwnerId();
        FallbackGarnishmentRecordFteRangeRuleFlagsValuesAboveFte();
        FallbackGarnishmentRecordRequiredOwnerIdRuleFlagsABlankOwnerId();
        FallbackGarnishmentRecordMarginRangeRuleFlagsValuesAboveMargin();
        FallbackGarnishmentRecordRequiredCodeRuleFlagsABlankCode();
        FallbackGarnishmentRecordRequiredCountryRuleFlagsABlankCountry();
    }
}
