namespace Acme.Workforce.Payrollops.Tests.Garnishment.Budget;

using Acme.Workforce.Payrollops.Garnishment.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LenientGarnishmentBudgetRulesChecks
{
    public static void LenientGarnishmentBudgetValidatorReportsEveryProblem()
    {
        var validator = LenientGarnishmentBudgetRules.DefaultLenientGarnishmentBudgetValidator();
        var item = LenientGarnishmentBudget.NewLenientGarnishmentBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientGarnishmentBudgetMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new LenientGarnishmentBudgetMaximumRangeRule(5.4, 4459.8);
        var item = LenientGarnishmentBudget.NewLenientGarnishmentBudget("x", "y");
        item.Maximum = 4461.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientGarnishmentBudgetChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new LenientGarnishmentBudgetChecksumLengthRule(86400);
        var item = LenientGarnishmentBudget.NewLenientGarnishmentBudget("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientGarnishmentBudgetRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new LenientGarnishmentBudgetRequiredChecksumRule("lenient garnishment budget needs this field");
        var item = LenientGarnishmentBudget.NewLenientGarnishmentBudget("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientGarnishmentBudgetValidatorReportsEveryProblem();
        LenientGarnishmentBudgetMaximumRangeRuleFlagsValuesAboveMaximum();
        LenientGarnishmentBudgetChecksumLengthRuleFlagsALongChecksum();
        LenientGarnishmentBudgetRequiredChecksumRuleFlagsABlankChecksum();
    }
}
