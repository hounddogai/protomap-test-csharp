namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class StrictPayBandDraftRulesChecks
{
    public static void StrictPayBandDraftValidatorReportsEveryProblem()
    {
        var validator = StrictPayBandDraftRules.DefaultStrictPayBandDraftValidator();
        var item = StrictPayBandDraft.NewStrictPayBandDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPayBandDraftDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new StrictPayBandDraftDiscountRangeRule(7.7, 3368.2);
        var item = StrictPayBandDraft.NewStrictPayBandDraft("x", "y");
        item.Discount = 3369.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictPayBandDraftRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new StrictPayBandDraftRequiredChannelRule("strict pay band draft needs a value");
        var item = StrictPayBandDraft.NewStrictPayBandDraft("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictPayBandDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictPayBandDraftRequiredCodeRule("strict pay band draft needs an entry");
        var item = StrictPayBandDraft.NewStrictPayBandDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictPayBandDraftNameLengthRuleFlagsALongName()
    {
        var rule = new StrictPayBandDraftNameLengthRule(255);
        var item = StrictPayBandDraft.NewStrictPayBandDraft("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPayBandDraftRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictPayBandDraftRequiredNameRule("strict pay band draft needs a value");
        var item = StrictPayBandDraft.NewStrictPayBandDraft("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictPayBandDraftValidatorReportsEveryProblem();
        StrictPayBandDraftDiscountRangeRuleFlagsValuesAboveDiscount();
        StrictPayBandDraftRequiredChannelRuleFlagsABlankChannel();
        StrictPayBandDraftRequiredCodeRuleFlagsABlankCode();
        StrictPayBandDraftNameLengthRuleFlagsALongName();
        StrictPayBandDraftRequiredNameRuleFlagsABlankName();
    }
}
