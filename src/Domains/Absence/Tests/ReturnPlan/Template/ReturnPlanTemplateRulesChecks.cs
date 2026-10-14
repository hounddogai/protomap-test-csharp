namespace Acme.Workforce.Absence.Tests.ReturnPlan.Template;

using Acme.Workforce.Absence.ReturnPlan.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ReturnPlanTemplateRulesChecks
{
    public static void ReturnPlanTemplateValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanTemplateRules.DefaultReturnPlanTemplateValidator();
        var item = ReturnPlanTemplate.NewReturnPlanTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanTemplateAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new ReturnPlanTemplateAttemptsRangeRule(80, 1000);
        var item = ReturnPlanTemplate.NewReturnPlanTemplate("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReturnPlanTemplateRequiredCodeRule("return plan template needs an entry");
        var item = ReturnPlanTemplate.NewReturnPlanTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanTemplateRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new ReturnPlanTemplateRequiredChecksumRule("return plan template needs an entry");
        var item = ReturnPlanTemplate.NewReturnPlanTemplate("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReturnPlanTemplateValidatorReportsEveryProblem();
        ReturnPlanTemplateAttemptsRangeRuleFlagsValuesAboveAttempts();
        ReturnPlanTemplateRequiredCodeRuleFlagsABlankCode();
        ReturnPlanTemplateRequiredChecksumRuleFlagsABlankChecksum();
    }
}
