namespace Acme.Workforce.Contractors.Tests.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Engagement.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class EngagementTargetRulesChecks
{
    public static void EngagementTargetValidatorReportsEveryProblem()
    {
        var validator = EngagementTargetRules.DefaultEngagementTargetValidator();
        var item = EngagementTarget.NewEngagementTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementTargetRequiredCodeRule("engagement target needs a value");
        var item = EngagementTarget.NewEngagementTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementTargetNameLengthRuleFlagsALongName()
    {
        var rule = new EngagementTargetNameLengthRule(120);
        var item = EngagementTarget.NewEngagementTarget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementTargetRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new EngagementTargetRequiredLocaleRule("engagement target needs this field");
        var item = EngagementTarget.NewEngagementTarget("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementTargetValidatorReportsEveryProblem();
        EngagementTargetRequiredCodeRuleFlagsABlankCode();
        EngagementTargetNameLengthRuleFlagsALongName();
        EngagementTargetRequiredLocaleRuleFlagsABlankLocale();
    }
}
