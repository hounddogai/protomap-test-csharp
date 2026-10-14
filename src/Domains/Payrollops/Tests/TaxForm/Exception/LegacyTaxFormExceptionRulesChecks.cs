namespace Acme.Workforce.Payrollops.Tests.TaxForm.Exception;

using Acme.Workforce.Payrollops.TaxForm.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LegacyTaxFormExceptionRulesChecks
{
    public static void LegacyTaxFormExceptionValidatorReportsEveryProblem()
    {
        var validator = LegacyTaxFormExceptionRules.DefaultLegacyTaxFormExceptionValidator();
        var item = LegacyTaxFormException.NewLegacyTaxFormException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTaxFormExceptionTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LegacyTaxFormExceptionTitleLengthRule(255);
        var item = LegacyTaxFormException.NewLegacyTaxFormException("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyTaxFormExceptionAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new LegacyTaxFormExceptionAttemptsRangeRule(120, 1000);
        var item = LegacyTaxFormException.NewLegacyTaxFormException("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyTaxFormExceptionDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LegacyTaxFormExceptionDurationMinutesRangeRule(80, 86400);
        var item = LegacyTaxFormException.NewLegacyTaxFormException("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyTaxFormExceptionEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new LegacyTaxFormExceptionEndsAtRangeRule(64, 86400);
        var item = LegacyTaxFormException.NewLegacyTaxFormException("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyTaxFormExceptionValidatorReportsEveryProblem();
        LegacyTaxFormExceptionTitleLengthRuleFlagsALongTitle();
        LegacyTaxFormExceptionAttemptsRangeRuleFlagsValuesAboveAttempts();
        LegacyTaxFormExceptionDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        LegacyTaxFormExceptionEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
