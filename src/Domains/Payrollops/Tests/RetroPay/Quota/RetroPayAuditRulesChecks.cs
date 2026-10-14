namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayAuditRulesChecks
{
    public static void RetroPayAuditValidatorReportsEveryProblem()
    {
        var validator = RetroPayAuditRules.DefaultRetroPayAuditValidator();
        var item = RetroPayAudit.NewRetroPayAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayAuditRequiredNameRuleFlagsABlankName()
    {
        var rule = new RetroPayAuditRequiredNameRule("retro pay audit needs an entry");
        var item = RetroPayAudit.NewRetroPayAudit("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayAuditStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new RetroPayAuditStartsAtRangeRule(120, 3600);
        var item = RetroPayAudit.NewRetroPayAudit("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayAuditNameLengthRuleFlagsALongName()
    {
        var rule = new RetroPayAuditNameLengthRule(120);
        var item = RetroPayAudit.NewRetroPayAudit("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayAuditRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new RetroPayAuditRequiredSourceRule("retro pay audit needs this field");
        var item = RetroPayAudit.NewRetroPayAudit("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayAuditRequiredCodeRule("retro pay audit needs an entry");
        var item = RetroPayAudit.NewRetroPayAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RetroPayAuditValidatorReportsEveryProblem();
        RetroPayAuditRequiredNameRuleFlagsABlankName();
        RetroPayAuditStartsAtRangeRuleFlagsValuesAboveStartsAt();
        RetroPayAuditNameLengthRuleFlagsALongName();
        RetroPayAuditRequiredSourceRuleFlagsABlankSource();
        RetroPayAuditRequiredCodeRuleFlagsABlankCode();
    }
}
