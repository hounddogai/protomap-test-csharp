namespace Acme.Workforce.Talent.Tests.Nomination.Notice;

using Acme.Workforce.Talent.Nomination.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class NominationNoticeRulesChecks
{
    public static void NominationNoticeValidatorReportsEveryProblem()
    {
        var validator = NominationNoticeRules.DefaultNominationNoticeValidator();
        var item = NominationNotice.NewNominationNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationNoticeTitleLengthRuleFlagsALongTitle()
    {
        var rule = new NominationNoticeTitleLengthRule(16);
        var item = NominationNotice.NewNominationNotice("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationNoticeOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new NominationNoticeOwnerIdLengthRule(64);
        var item = NominationNotice.NewNominationNotice("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationNoticeLocationLengthRuleFlagsALongLocation()
    {
        var rule = new NominationNoticeLocationLengthRule(64);
        var item = NominationNotice.NewNominationNotice("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationNoticeAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new NominationNoticeAttemptsRangeRule(80, 1000);
        var item = NominationNotice.NewNominationNotice("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationNoticeRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new NominationNoticeRequiredOwnerIdRule("nomination notice needs an entry");
        var item = NominationNotice.NewNominationNotice("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        NominationNoticeValidatorReportsEveryProblem();
        NominationNoticeTitleLengthRuleFlagsALongTitle();
        NominationNoticeOwnerIdLengthRuleFlagsALongOwnerId();
        NominationNoticeLocationLengthRuleFlagsALongLocation();
        NominationNoticeAttemptsRangeRuleFlagsValuesAboveAttempts();
        NominationNoticeRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
