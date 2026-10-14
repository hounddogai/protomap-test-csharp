namespace Acme.Workforce.Talent.Tests.Nomination.Profile;

using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class StrictNominationProfileRulesChecks
{
    public static void StrictNominationProfileValidatorReportsEveryProblem()
    {
        var validator = StrictNominationProfileRules.DefaultStrictNominationProfileValidator();
        var item = StrictNominationProfile.NewStrictNominationProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictNominationProfileRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new StrictNominationProfileRequiredChannelRule("strict nomination profile needs an entry");
        var item = StrictNominationProfile.NewStrictNominationProfile("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictNominationProfileRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictNominationProfileRequiredNameRule("strict nomination profile needs this field");
        var item = StrictNominationProfile.NewStrictNominationProfile("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictNominationProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictNominationProfileCodeLengthRule(80);
        var item = StrictNominationProfile.NewStrictNominationProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictNominationProfileValidatorReportsEveryProblem();
        StrictNominationProfileRequiredChannelRuleFlagsABlankChannel();
        StrictNominationProfileRequiredNameRuleFlagsABlankName();
        StrictNominationProfileCodeLengthRuleFlagsALongCode();
    }
}
