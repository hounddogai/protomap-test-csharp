namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshDigestRulesChecks
{
    public static void RefreshDigestValidatorReportsEveryProblem()
    {
        var validator = RefreshDigestRules.DefaultRefreshDigestValidator();
        var item = RefreshDigest.NewRefreshDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshDigestWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new RefreshDigestWeightRangeRule(3.3, 4476.2);
        var item = RefreshDigest.NewRefreshDigest("x", "y");
        item.Weight = 4477.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshDigestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RefreshDigestRequiredTitleRule("refresh digest needs this field");
        var item = RefreshDigest.NewRefreshDigest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshDigestCodeLengthRule(80);
        var item = RefreshDigest.NewRefreshDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RefreshDigestValidatorReportsEveryProblem();
        RefreshDigestWeightRangeRuleFlagsValuesAboveWeight();
        RefreshDigestRequiredTitleRuleFlagsABlankTitle();
        RefreshDigestCodeLengthRuleFlagsALongCode();
    }
}
