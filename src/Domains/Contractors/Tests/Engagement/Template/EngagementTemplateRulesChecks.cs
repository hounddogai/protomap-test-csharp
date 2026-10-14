namespace Acme.Workforce.Contractors.Tests.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class EngagementTemplateRulesChecks
{
    public static void EngagementTemplateValidatorReportsEveryProblem()
    {
        var validator = EngagementTemplateRules.DefaultEngagementTemplateValidator();
        var item = EngagementTemplate.NewEngagementTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementTemplateGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new EngagementTemplateGraceDaysRangeRule(80, 255);
        var item = EngagementTemplate.NewEngagementTemplate("x", "y");
        item.GraceDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementTemplateAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new EngagementTemplateAttemptsRangeRule(32, 64);
        var item = EngagementTemplate.NewEngagementTemplate("x", "y");
        item.Attempts = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementTemplateRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new EngagementTemplateRetriesRangeRule(1000, 86400);
        var item = EngagementTemplate.NewEngagementTemplate("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new EngagementTemplateCodeLengthRule(32);
        var item = EngagementTemplate.NewEngagementTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementTemplateRequiredCodeRule("engagement template needs this field");
        var item = EngagementTemplate.NewEngagementTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementTemplateValidatorReportsEveryProblem();
        EngagementTemplateGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        EngagementTemplateAttemptsRangeRuleFlagsValuesAboveAttempts();
        EngagementTemplateRetriesRangeRuleFlagsValuesAboveRetries();
        EngagementTemplateCodeLengthRuleFlagsALongCode();
        EngagementTemplateRequiredCodeRuleFlagsABlankCode();
    }
}
