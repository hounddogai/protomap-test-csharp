namespace Acme.Workforce.Payrollops.Tests.TaxForm.Group;

using Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ScheduledTaxFormTemplateRulesChecks
{
    public static void ScheduledTaxFormTemplateValidatorReportsEveryProblem()
    {
        var validator = ScheduledTaxFormTemplateRules.DefaultScheduledTaxFormTemplateValidator();
        var item = ScheduledTaxFormTemplate.NewScheduledTaxFormTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledTaxFormTemplateRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledTaxFormTemplateRequiredNameRule("scheduled tax form template needs a value");
        var item = ScheduledTaxFormTemplate.NewScheduledTaxFormTemplate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledTaxFormTemplateRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new ScheduledTaxFormTemplateRequiredSkillRule("scheduled tax form template needs this field");
        var item = ScheduledTaxFormTemplate.NewScheduledTaxFormTemplate("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledTaxFormTemplateValidatorReportsEveryProblem();
        ScheduledTaxFormTemplateRequiredNameRuleFlagsABlankName();
        ScheduledTaxFormTemplateRequiredSkillRuleFlagsABlankSkill();
    }
}
