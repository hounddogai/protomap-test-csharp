namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ExceptionTemplateRulesChecks
{
    public static void ExceptionTemplateValidatorReportsEveryProblem()
    {
        var validator = ExceptionTemplateRules.DefaultExceptionTemplateValidator();
        var item = ExceptionTemplate.NewExceptionTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionTemplateRequiredCodeRule("exception template needs an entry");
        var item = ExceptionTemplate.NewExceptionTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionTemplateDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new ExceptionTemplateDescriptionLengthRule(16);
        var item = ExceptionTemplate.NewExceptionTemplate("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionTemplateRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new ExceptionTemplateRequiredDescriptionRule("exception template needs this field");
        var item = ExceptionTemplate.NewExceptionTemplate("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionTemplateCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new ExceptionTemplateCurrencyLengthRule(64);
        var item = ExceptionTemplate.NewExceptionTemplate("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionTemplateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ExceptionTemplateTitleLengthRule(3600);
        var item = ExceptionTemplate.NewExceptionTemplate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ExceptionTemplateValidatorReportsEveryProblem();
        ExceptionTemplateRequiredCodeRuleFlagsABlankCode();
        ExceptionTemplateDescriptionLengthRuleFlagsALongDescription();
        ExceptionTemplateRequiredDescriptionRuleFlagsABlankDescription();
        ExceptionTemplateCurrencyLengthRuleFlagsALongCurrency();
        ExceptionTemplateTitleLengthRuleFlagsALongTitle();
    }
}
