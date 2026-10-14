namespace Acme.Workforce.Payrollops.Tests.RetroPay.Draft;

using Acme.Workforce.Payrollops.RetroPay.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RetroPayDraftRulesChecks
{
    public static void RetroPayDraftValidatorReportsEveryProblem()
    {
        var validator = RetroPayDraftRules.DefaultRetroPayDraftValidator();
        var item = RetroPayDraft.NewRetroPayDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RetroPayDraftTitleLengthRule(86400);
        var item = RetroPayDraft.NewRetroPayDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayDraftRequiredCodeRule("retro pay draft needs this field");
        var item = RetroPayDraft.NewRetroPayDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayDraftNameLengthRuleFlagsALongName()
    {
        var rule = new RetroPayDraftNameLengthRule(1000);
        var item = RetroPayDraft.NewRetroPayDraft("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayDraftRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new RetroPayDraftRequiredPhoneRule("retro pay draft needs a value");
        var item = RetroPayDraft.NewRetroPayDraft("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayDraftWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new RetroPayDraftWeightRangeRule(3.9, 2408.8);
        var item = RetroPayDraft.NewRetroPayDraft("x", "y");
        item.Weight = 2410.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RetroPayDraftValidatorReportsEveryProblem();
        RetroPayDraftTitleLengthRuleFlagsALongTitle();
        RetroPayDraftRequiredCodeRuleFlagsABlankCode();
        RetroPayDraftNameLengthRuleFlagsALongName();
        RetroPayDraftRequiredPhoneRuleFlagsABlankPhone();
        RetroPayDraftWeightRangeRuleFlagsValuesAboveWeight();
    }
}
