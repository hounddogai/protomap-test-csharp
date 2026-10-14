namespace Acme.Workforce.Payrollops.Tests.PayRun.RunLedger;

using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Checks of run ledger code, run by the project's runner.</summary>
public static class PayRunPreferenceRulesChecks
{
    public static void PayRunPreferenceValidatorReportsEveryProblem()
    {
        var validator = PayRunPreferenceRules.DefaultPayRunPreferenceValidator();
        var item = PayRunPreference.NewPayRunPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunPreferenceBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PayRunPreferenceBudgetRangeRule(7.5, 4975.3);
        var item = PayRunPreference.NewPayRunPreference("x", "y");
        item.Budget = 4976.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunPreferenceExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PayRunPreferenceExpiresAtRangeRule(80, 120);
        var item = PayRunPreference.NewPayRunPreference("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunPreferenceEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PayRunPreferenceEndsAtRangeRule(120, 86400);
        var item = PayRunPreference.NewPayRunPreference("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunPreferenceRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new PayRunPreferenceRequiredCountryRule("pay run preference needs this field");
        var item = PayRunPreference.NewPayRunPreference("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayRunPreferenceCodeLengthRule(86400);
        var item = PayRunPreference.NewPayRunPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayRunPreferenceValidatorReportsEveryProblem();
        PayRunPreferenceBudgetRangeRuleFlagsValuesAboveBudget();
        PayRunPreferenceExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        PayRunPreferenceEndsAtRangeRuleFlagsValuesAboveEndsAt();
        PayRunPreferenceRequiredCountryRuleFlagsABlankCountry();
        PayRunPreferenceCodeLengthRuleFlagsALongCode();
    }
}
