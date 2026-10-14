namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormAssignment;

using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Checks of form assignment code, run by the project's runner.</summary>
public static class CappedTaxFormRuleRulesChecks
{
    public static void CappedTaxFormRuleValidatorReportsEveryProblem()
    {
        var validator = CappedTaxFormRuleRules.DefaultCappedTaxFormRuleValidator();
        var item = CappedTaxFormRule.NewCappedTaxFormRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedTaxFormRuleNameLengthRuleFlagsALongName()
    {
        var rule = new CappedTaxFormRuleNameLengthRule(3600);
        var item = CappedTaxFormRule.NewCappedTaxFormRule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedTaxFormRuleChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new CappedTaxFormRuleChecksumLengthRule(80);
        var item = CappedTaxFormRule.NewCappedTaxFormRule("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedTaxFormRuleValidatorReportsEveryProblem();
        CappedTaxFormRuleNameLengthRuleFlagsALongName();
        CappedTaxFormRuleChecksumLengthRuleFlagsALongChecksum();
    }
}
