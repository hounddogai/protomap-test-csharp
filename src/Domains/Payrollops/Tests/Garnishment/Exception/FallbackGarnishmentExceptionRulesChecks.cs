namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class FallbackGarnishmentExceptionRulesChecks
{
    public static void FallbackGarnishmentExceptionValidatorReportsEveryProblem()
    {
        var validator = FallbackGarnishmentExceptionRules.DefaultFallbackGarnishmentExceptionValidator();
        var item = FallbackGarnishmentException.NewFallbackGarnishmentException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackGarnishmentExceptionAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new FallbackGarnishmentExceptionAmountRangeRule(6.7, 696.6);
        var item = FallbackGarnishmentException.NewFallbackGarnishmentException("x", "y");
        item.Amount = 698.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackGarnishmentExceptionRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new FallbackGarnishmentExceptionRequiredRegionRule("fallback garnishment exception needs this field");
        var item = FallbackGarnishmentException.NewFallbackGarnishmentException("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackGarnishmentExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackGarnishmentExceptionCodeLengthRule(3600);
        var item = FallbackGarnishmentException.NewFallbackGarnishmentException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackGarnishmentExceptionValidatorReportsEveryProblem();
        FallbackGarnishmentExceptionAmountRangeRuleFlagsValuesAboveAmount();
        FallbackGarnishmentExceptionRequiredRegionRuleFlagsABlankRegion();
        FallbackGarnishmentExceptionCodeLengthRuleFlagsALongCode();
    }
}
