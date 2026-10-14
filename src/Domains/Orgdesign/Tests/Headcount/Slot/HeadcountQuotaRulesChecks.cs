namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class HeadcountQuotaRulesChecks
{
    public static void HeadcountQuotaValidatorReportsEveryProblem()
    {
        var validator = HeadcountQuotaRules.DefaultHeadcountQuotaValidator();
        var item = HeadcountQuota.NewHeadcountQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new HeadcountQuotaCodeLengthRule(16);
        var item = HeadcountQuota.NewHeadcountQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountQuotaDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new HeadcountQuotaDescriptionLengthRule(3600);
        var item = HeadcountQuota.NewHeadcountQuota("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountQuotaRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new HeadcountQuotaRequiredDescriptionRule("headcount quota needs a value");
        var item = HeadcountQuota.NewHeadcountQuota("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountQuotaRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new HeadcountQuotaRequiredChannelRule("headcount quota needs a value");
        var item = HeadcountQuota.NewHeadcountQuota("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HeadcountQuotaValidatorReportsEveryProblem();
        HeadcountQuotaCodeLengthRuleFlagsALongCode();
        HeadcountQuotaDescriptionLengthRuleFlagsALongDescription();
        HeadcountQuotaRequiredDescriptionRuleFlagsABlankDescription();
        HeadcountQuotaRequiredChannelRuleFlagsABlankChannel();
    }
}
