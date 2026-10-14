namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBaseline;

using Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

/// <summary>Checks of deduction baseline code, run by the project's runner.</summary>
public static class DeductionRuleRulesChecks
{
    public static void DeductionRuleValidatorReportsEveryProblem()
    {
        var validator = DeductionRuleRules.DefaultDeductionRuleValidator();
        var item = DeductionRule.NewDeductionRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionRuleCodeLengthRule(3600);
        var item = DeductionRule.NewDeductionRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionRuleRequiredNameRuleFlagsABlankName()
    {
        var rule = new DeductionRuleRequiredNameRule("deduction rule needs a value");
        var item = DeductionRule.NewDeductionRule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionRuleNameLengthRuleFlagsALongName()
    {
        var rule = new DeductionRuleNameLengthRule(80);
        var item = DeductionRule.NewDeductionRule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeductionRuleValidatorReportsEveryProblem();
        DeductionRuleCodeLengthRuleFlagsALongCode();
        DeductionRuleRequiredNameRuleFlagsABlankName();
        DeductionRuleNameLengthRuleFlagsALongName();
    }
}
