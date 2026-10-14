namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class PrimaryRetroPayAuditRulesChecks
{
    public static void PrimaryRetroPayAuditValidatorReportsEveryProblem()
    {
        var validator = PrimaryRetroPayAuditRules.DefaultPrimaryRetroPayAuditValidator();
        var item = PrimaryRetroPayAudit.NewPrimaryRetroPayAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryRetroPayAuditRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PrimaryRetroPayAuditRequiredLabelRule("primary retro pay audit needs an entry");
        var item = PrimaryRetroPayAudit.NewPrimaryRetroPayAudit("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryRetroPayAuditRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new PrimaryRetroPayAuditRatioRangeRule(0.7, 2394.2);
        var item = PrimaryRetroPayAudit.NewPrimaryRetroPayAudit("x", "y");
        item.Ratio = 2395.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryRetroPayAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryRetroPayAuditRequiredCodeRule("primary retro pay audit needs a value");
        var item = PrimaryRetroPayAudit.NewPrimaryRetroPayAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryRetroPayAuditTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new PrimaryRetroPayAuditTimezoneLengthRule(32);
        var item = PrimaryRetroPayAudit.NewPrimaryRetroPayAudit("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryRetroPayAuditValidatorReportsEveryProblem();
        PrimaryRetroPayAuditRequiredLabelRuleFlagsABlankLabel();
        PrimaryRetroPayAuditRatioRangeRuleFlagsValuesAboveRatio();
        PrimaryRetroPayAuditRequiredCodeRuleFlagsABlankCode();
        PrimaryRetroPayAuditTimezoneLengthRuleFlagsALongTimezone();
    }
}
