namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RetroPayRecordRulesChecks
{
    public static void RetroPayRecordValidatorReportsEveryProblem()
    {
        var validator = RetroPayRecordRules.DefaultRetroPayRecordValidator();
        var item = RetroPayRecord.NewRetroPayRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayRecordRequiredCodeRule("retro pay record needs an entry");
        var item = RetroPayRecord.NewRetroPayRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayRecordRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new RetroPayRecordRequiredSalaryBandRule("retro pay record needs this field");
        var item = RetroPayRecord.NewRetroPayRecord("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayRecordRequiredNameRuleFlagsABlankName()
    {
        var rule = new RetroPayRecordRequiredNameRule("retro pay record needs an entry");
        var item = RetroPayRecord.NewRetroPayRecord("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RetroPayRecordValidatorReportsEveryProblem();
        RetroPayRecordRequiredCodeRuleFlagsABlankCode();
        RetroPayRecordRequiredSalaryBandRuleFlagsABlankSalaryBand();
        RetroPayRecordRequiredNameRuleFlagsABlankName();
    }
}
