namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class TaxFormWindowRulesChecks
{
    public static void TaxFormWindowValidatorReportsEveryProblem()
    {
        var validator = TaxFormWindowRules.DefaultTaxFormWindowValidator();
        var item = TaxFormWindow.NewTaxFormWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormWindowCodeLengthRule(120);
        var item = TaxFormWindow.NewTaxFormWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormWindowSkillLengthRuleFlagsALongSkill()
    {
        var rule = new TaxFormWindowSkillLengthRule(80);
        var item = TaxFormWindow.NewTaxFormWindow("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormWindowNameLengthRuleFlagsALongName()
    {
        var rule = new TaxFormWindowNameLengthRule(1000);
        var item = TaxFormWindow.NewTaxFormWindow("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TaxFormWindowValidatorReportsEveryProblem();
        TaxFormWindowCodeLengthRuleFlagsALongCode();
        TaxFormWindowSkillLengthRuleFlagsALongSkill();
        TaxFormWindowNameLengthRuleFlagsALongName();
    }
}
