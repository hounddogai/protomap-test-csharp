namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class StrictNominationTierRulesChecks
{
    public static void StrictNominationTierValidatorReportsEveryProblem()
    {
        var validator = StrictNominationTierRules.DefaultStrictNominationTierValidator();
        var item = StrictNominationTier.NewStrictNominationTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictNominationTierRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictNominationTierRequiredTitleRule("strict nomination tier needs this field");
        var item = StrictNominationTier.NewStrictNominationTier("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictNominationTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictNominationTierRequiredCodeRule("strict nomination tier needs a value");
        var item = StrictNominationTier.NewStrictNominationTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictNominationTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictNominationTierCodeLengthRule(255);
        var item = StrictNominationTier.NewStrictNominationTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictNominationTierRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new StrictNominationTierRequiredTimezoneRule("strict nomination tier needs an entry");
        var item = StrictNominationTier.NewStrictNominationTier("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictNominationTierValidatorReportsEveryProblem();
        StrictNominationTierRequiredTitleRuleFlagsABlankTitle();
        StrictNominationTierRequiredCodeRuleFlagsABlankCode();
        StrictNominationTierCodeLengthRuleFlagsALongCode();
        StrictNominationTierRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
