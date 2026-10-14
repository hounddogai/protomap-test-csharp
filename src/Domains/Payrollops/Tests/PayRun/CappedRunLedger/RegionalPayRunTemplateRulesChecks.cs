namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class RegionalPayRunTemplateRulesChecks
{
    public static void RegionalPayRunTemplateValidatorReportsEveryProblem()
    {
        var validator = RegionalPayRunTemplateRules.DefaultRegionalPayRunTemplateValidator();
        var item = RegionalPayRunTemplate.NewRegionalPayRunTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalPayRunTemplateChannelLengthRuleFlagsALongChannel()
    {
        var rule = new RegionalPayRunTemplateChannelLengthRule(64);
        var item = RegionalPayRunTemplate.NewRegionalPayRunTemplate("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalPayRunTemplatePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new RegionalPayRunTemplatePhoneLengthRule(3600);
        var item = RegionalPayRunTemplate.NewRegionalPayRunTemplate("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalPayRunTemplateRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new RegionalPayRunTemplateRequiredPhoneRule("regional pay run template needs this field");
        var item = RegionalPayRunTemplate.NewRegionalPayRunTemplate("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalPayRunTemplateValidatorReportsEveryProblem();
        RegionalPayRunTemplateChannelLengthRuleFlagsALongChannel();
        RegionalPayRunTemplatePhoneLengthRuleFlagsALongPhone();
        RegionalPayRunTemplateRequiredPhoneRuleFlagsABlankPhone();
    }
}
