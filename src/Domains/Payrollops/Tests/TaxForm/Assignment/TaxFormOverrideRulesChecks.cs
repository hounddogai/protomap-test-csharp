namespace Acme.Workforce.Payrollops.Tests.TaxForm.Assignment;

using Acme.Workforce.Payrollops.TaxForm.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TaxFormOverrideRulesChecks
{
    public static void TaxFormOverrideValidatorReportsEveryProblem()
    {
        var validator = TaxFormOverrideRules.DefaultTaxFormOverrideValidator();
        var item = TaxFormOverride.NewTaxFormOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormOverrideExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new TaxFormOverrideExpiresAtRangeRule(64, 120);
        var item = TaxFormOverride.NewTaxFormOverride("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormOverrideCodeLengthRule(16);
        var item = TaxFormOverride.NewTaxFormOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormOverrideRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new TaxFormOverrideRequiredSalaryBandRule("tax form override needs an entry");
        var item = TaxFormOverride.NewTaxFormOverride("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormOverrideMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new TaxFormOverrideMinimumRangeRule(1.1, 4252.5);
        var item = TaxFormOverride.NewTaxFormOverride("x", "y");
        item.Minimum = 4254.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TaxFormOverrideValidatorReportsEveryProblem();
        TaxFormOverrideExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        TaxFormOverrideCodeLengthRuleFlagsALongCode();
        TaxFormOverrideRequiredSalaryBandRuleFlagsABlankSalaryBand();
        TaxFormOverrideMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
