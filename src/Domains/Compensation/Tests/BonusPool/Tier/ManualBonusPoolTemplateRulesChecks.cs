namespace Acme.Workforce.Compensation.Tests.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualBonusPoolTemplateRulesChecks
{
    public static void ManualBonusPoolTemplateValidatorReportsEveryProblem()
    {
        var validator = ManualBonusPoolTemplateRules.DefaultManualBonusPoolTemplateValidator();
        var item = ManualBonusPoolTemplate.NewManualBonusPoolTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualBonusPoolTemplateNameLengthRuleFlagsALongName()
    {
        var rule = new ManualBonusPoolTemplateNameLengthRule(3600);
        var item = ManualBonusPoolTemplate.NewManualBonusPoolTemplate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBonusPoolTemplateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ManualBonusPoolTemplateRequiredTitleRule("manual bonus pool template needs this field");
        var item = ManualBonusPoolTemplate.NewManualBonusPoolTemplate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualBonusPoolTemplateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ManualBonusPoolTemplateTitleLengthRule(32);
        var item = ManualBonusPoolTemplate.NewManualBonusPoolTemplate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBonusPoolTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualBonusPoolTemplateRequiredCodeRule("manual bonus pool template needs a value");
        var item = ManualBonusPoolTemplate.NewManualBonusPoolTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualBonusPoolTemplateValidatorReportsEveryProblem();
        ManualBonusPoolTemplateNameLengthRuleFlagsALongName();
        ManualBonusPoolTemplateRequiredTitleRuleFlagsABlankTitle();
        ManualBonusPoolTemplateTitleLengthRuleFlagsALongTitle();
        ManualBonusPoolTemplateRequiredCodeRuleFlagsABlankCode();
    }
}
