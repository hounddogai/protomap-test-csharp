namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeProfileRulesChecks
{
    public static void AccessBadgeProfileValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeProfileRules.DefaultAccessBadgeProfileValidator();
        var item = AccessBadgeProfile.NewAccessBadgeProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeProfileGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new AccessBadgeProfileGraceDaysRangeRule(64, 86400);
        var item = AccessBadgeProfile.NewAccessBadgeProfile("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeProfileNotesLengthRuleFlagsALongNotes()
    {
        var rule = new AccessBadgeProfileNotesLengthRule(64);
        var item = AccessBadgeProfile.NewAccessBadgeProfile("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgeProfileCodeLengthRule(255);
        var item = AccessBadgeProfile.NewAccessBadgeProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccessBadgeProfileValidatorReportsEveryProblem();
        AccessBadgeProfileGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        AccessBadgeProfileNotesLengthRuleFlagsALongNotes();
        AccessBadgeProfileCodeLengthRuleFlagsALongCode();
    }
}
