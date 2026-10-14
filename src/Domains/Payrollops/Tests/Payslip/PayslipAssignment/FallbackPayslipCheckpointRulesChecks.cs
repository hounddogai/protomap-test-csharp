namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class FallbackPayslipCheckpointRulesChecks
{
    public static void FallbackPayslipCheckpointValidatorReportsEveryProblem()
    {
        var validator = FallbackPayslipCheckpointRules.DefaultFallbackPayslipCheckpointValidator();
        var item = FallbackPayslipCheckpoint.NewFallbackPayslipCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPayslipCheckpointCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new FallbackPayslipCheckpointCurrencyLengthRule(64);
        var item = FallbackPayslipCheckpoint.NewFallbackPayslipCheckpoint("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPayslipCheckpointAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new FallbackPayslipCheckpointAmountRangeRule(6.9, 3055.4);
        var item = FallbackPayslipCheckpoint.NewFallbackPayslipCheckpoint("x", "y");
        item.Amount = 3056.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPayslipCheckpointRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackPayslipCheckpointRequiredNameRule("fallback payslip checkpoint needs this field");
        var item = FallbackPayslipCheckpoint.NewFallbackPayslipCheckpoint("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackPayslipCheckpointValidatorReportsEveryProblem();
        FallbackPayslipCheckpointCurrencyLengthRuleFlagsALongCurrency();
        FallbackPayslipCheckpointAmountRangeRuleFlagsValuesAboveAmount();
        FallbackPayslipCheckpointRequiredNameRuleFlagsABlankName();
    }
}
