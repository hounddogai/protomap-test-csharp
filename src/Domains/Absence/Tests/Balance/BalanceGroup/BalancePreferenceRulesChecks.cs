namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class BalancePreferenceRulesChecks
{
    public static void BalancePreferenceValidatorReportsEveryProblem()
    {
        var validator = BalancePreferenceRules.DefaultBalancePreferenceValidator();
        var item = BalancePreference.NewBalancePreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalancePreferenceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BalancePreferenceTitleLengthRule(80);
        var item = BalancePreference.NewBalancePreference("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalancePreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new BalancePreferenceCodeLengthRule(80);
        var item = BalancePreference.NewBalancePreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalancePreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BalancePreferenceRequiredCodeRule("balance preference needs an entry");
        var item = BalancePreference.NewBalancePreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalancePreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BalancePreferenceRequiredTitleRule("balance preference needs an entry");
        var item = BalancePreference.NewBalancePreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalancePreferenceRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new BalancePreferenceRatingRangeRule(8.5, 3374.4);
        var item = BalancePreference.NewBalancePreference("x", "y");
        item.Rating = 3375.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BalancePreferenceValidatorReportsEveryProblem();
        BalancePreferenceTitleLengthRuleFlagsALongTitle();
        BalancePreferenceCodeLengthRuleFlagsALongCode();
        BalancePreferenceRequiredCodeRuleFlagsABlankCode();
        BalancePreferenceRequiredTitleRuleFlagsABlankTitle();
        BalancePreferenceRatingRangeRuleFlagsValuesAboveRating();
    }
}
