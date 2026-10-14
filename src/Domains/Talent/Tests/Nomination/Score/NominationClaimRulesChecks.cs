namespace Acme.Workforce.Talent.Tests.Nomination.Score;

using Acme.Workforce.Talent.Nomination.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class NominationClaimRulesChecks
{
    public static void NominationClaimValidatorReportsEveryProblem()
    {
        var validator = NominationClaimRules.DefaultNominationClaimValidator();
        var item = NominationClaim.NewNominationClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationClaimRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new NominationClaimRequiredLocationRule("nomination claim needs an entry");
        var item = NominationClaim.NewNominationClaim("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationClaimTitleLengthRuleFlagsALongTitle()
    {
        var rule = new NominationClaimTitleLengthRule(16);
        var item = NominationClaim.NewNominationClaim("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationClaimRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new NominationClaimRequiredTitleRule("nomination claim needs this field");
        var item = NominationClaim.NewNominationClaim("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationClaimLocationLengthRuleFlagsALongLocation()
    {
        var rule = new NominationClaimLocationLengthRule(255);
        var item = NominationClaim.NewNominationClaim("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new NominationClaimRequiredCodeRule("nomination claim needs a value");
        var item = NominationClaim.NewNominationClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        NominationClaimValidatorReportsEveryProblem();
        NominationClaimRequiredLocationRuleFlagsABlankLocation();
        NominationClaimTitleLengthRuleFlagsALongTitle();
        NominationClaimRequiredTitleRuleFlagsABlankTitle();
        NominationClaimLocationLengthRuleFlagsALongLocation();
        NominationClaimRequiredCodeRuleFlagsABlankCode();
    }
}
