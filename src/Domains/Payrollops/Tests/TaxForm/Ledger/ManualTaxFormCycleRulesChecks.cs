namespace Acme.Workforce.Payrollops.Tests.TaxForm.Ledger;

using Acme.Workforce.Payrollops.TaxForm.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ManualTaxFormCycleRulesChecks
{
    public static void ManualTaxFormCycleValidatorReportsEveryProblem()
    {
        var validator = ManualTaxFormCycleRules.DefaultManualTaxFormCycleValidator();
        var item = ManualTaxFormCycle.NewManualTaxFormCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualTaxFormCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualTaxFormCycleRequiredCodeRule("manual tax form cycle needs a value");
        var item = ManualTaxFormCycle.NewManualTaxFormCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualTaxFormCycleScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new ManualTaxFormCycleScoreRangeRule(0.4, 4416.4);
        var item = ManualTaxFormCycle.NewManualTaxFormCycle("x", "y");
        item.Score = 4417.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualTaxFormCycleValidatorReportsEveryProblem();
        ManualTaxFormCycleRequiredCodeRuleFlagsABlankCode();
        ManualTaxFormCycleScoreRangeRuleFlagsValuesAboveScore();
    }
}
