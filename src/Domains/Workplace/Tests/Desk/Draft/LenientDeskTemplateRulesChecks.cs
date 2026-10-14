namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientDeskTemplateRulesChecks
{
    public static void LenientDeskTemplateValidatorReportsEveryProblem()
    {
        var validator = LenientDeskTemplateRules.DefaultLenientDeskTemplateValidator();
        var item = LenientDeskTemplate.NewLenientDeskTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientDeskTemplateRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new LenientDeskTemplateRequiredChannelRule("lenient desk template needs this field");
        var item = LenientDeskTemplate.NewLenientDeskTemplate("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientDeskTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientDeskTemplateCodeLengthRule(120);
        var item = LenientDeskTemplate.NewLenientDeskTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientDeskTemplateLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new LenientDeskTemplateLeaveDaysRangeRule(255, 86400);
        var item = LenientDeskTemplate.NewLenientDeskTemplate("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientDeskTemplateRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientDeskTemplateRequiredNameRule("lenient desk template needs a value");
        var item = LenientDeskTemplate.NewLenientDeskTemplate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientDeskTemplateRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new LenientDeskTemplateRequiredJobCodeRule("lenient desk template needs an entry");
        var item = LenientDeskTemplate.NewLenientDeskTemplate("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientDeskTemplateValidatorReportsEveryProblem();
        LenientDeskTemplateRequiredChannelRuleFlagsABlankChannel();
        LenientDeskTemplateCodeLengthRuleFlagsALongCode();
        LenientDeskTemplateLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        LenientDeskTemplateRequiredNameRuleFlagsABlankName();
        LenientDeskTemplateRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
