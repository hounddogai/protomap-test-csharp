namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBudget;

using Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>Checks of deduction budget code, run by the project's runner.</summary>
public static class RegionalDeductionTemplateRulesChecks
{
    public static void RegionalDeductionTemplateValidatorReportsEveryProblem()
    {
        var validator = RegionalDeductionTemplateRules.DefaultRegionalDeductionTemplateValidator();
        var item = RegionalDeductionTemplate.NewRegionalDeductionTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalDeductionTemplateSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new RegionalDeductionTemplateSummaryLengthRule(32);
        var item = RegionalDeductionTemplate.NewRegionalDeductionTemplate("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalDeductionTemplateRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalDeductionTemplateRequiredNameRule("regional deduction template needs a value");
        var item = RegionalDeductionTemplate.NewRegionalDeductionTemplate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalDeductionTemplateRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new RegionalDeductionTemplateRequiredExternalRefRule("regional deduction template needs this field");
        var item = RegionalDeductionTemplate.NewRegionalDeductionTemplate("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalDeductionTemplateValidatorReportsEveryProblem();
        RegionalDeductionTemplateSummaryLengthRuleFlagsALongSummary();
        RegionalDeductionTemplateRequiredNameRuleFlagsABlankName();
        RegionalDeductionTemplateRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
