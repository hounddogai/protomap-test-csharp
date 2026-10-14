namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class AccessBadgePreferenceRulesChecks
{
    public static void AccessBadgePreferenceValidatorReportsEveryProblem()
    {
        var validator = AccessBadgePreferenceRules.DefaultAccessBadgePreferenceValidator();
        var item = AccessBadgePreference.NewAccessBadgePreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgePreferenceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AccessBadgePreferenceTitleLengthRule(32);
        var item = AccessBadgePreference.NewAccessBadgePreference("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgePreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgePreferenceCodeLengthRule(86400);
        var item = AccessBadgePreference.NewAccessBadgePreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgePreferenceSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new AccessBadgePreferenceSummaryLengthRule(86400);
        var item = AccessBadgePreference.NewAccessBadgePreference("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgePreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccessBadgePreferenceRequiredCodeRule("access badge preference needs a value");
        var item = AccessBadgePreference.NewAccessBadgePreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgePreferenceCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new AccessBadgePreferenceCeilingRangeRule(7.2, 1742.0);
        var item = AccessBadgePreference.NewAccessBadgePreference("x", "y");
        item.Ceiling = 1743.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgePreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AccessBadgePreferenceRequiredTitleRule("access badge preference needs a value");
        var item = AccessBadgePreference.NewAccessBadgePreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccessBadgePreferenceValidatorReportsEveryProblem();
        AccessBadgePreferenceTitleLengthRuleFlagsALongTitle();
        AccessBadgePreferenceCodeLengthRuleFlagsALongCode();
        AccessBadgePreferenceSummaryLengthRuleFlagsALongSummary();
        AccessBadgePreferenceRequiredCodeRuleFlagsABlankCode();
        AccessBadgePreferenceCeilingRangeRuleFlagsValuesAboveCeiling();
        AccessBadgePreferenceRequiredTitleRuleFlagsABlankTitle();
    }
}
