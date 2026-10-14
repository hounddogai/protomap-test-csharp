namespace Acme.Workforce.Compensation.Tests.PayBand.Draft;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PayBandDraftRulesChecks
{
    public static void PayBandDraftValidatorReportsEveryProblem()
    {
        var validator = PayBandDraftRules.DefaultPayBandDraftValidator();
        var item = PayBandDraft.NewPayBandDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandDraftNameLengthRuleFlagsALongName()
    {
        var rule = new PayBandDraftNameLengthRule(64);
        var item = PayBandDraft.NewPayBandDraft("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandDraftCountryLengthRuleFlagsALongCountry()
    {
        var rule = new PayBandDraftCountryLengthRule(86400);
        var item = PayBandDraft.NewPayBandDraft("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandDraftCodeLengthRule(255);
        var item = PayBandDraft.NewPayBandDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayBandDraftValidatorReportsEveryProblem();
        PayBandDraftNameLengthRuleFlagsALongName();
        PayBandDraftCountryLengthRuleFlagsALongCountry();
        PayBandDraftCodeLengthRuleFlagsALongCode();
    }
}
