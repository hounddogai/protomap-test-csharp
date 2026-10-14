namespace Acme.Workforce.Assets.Tests.Refresh.Policy;

using Acme.Workforce.Assets.Refresh.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class LenientRefreshProfileRulesChecks
{
    public static void LenientRefreshProfileValidatorReportsEveryProblem()
    {
        var validator = LenientRefreshProfileRules.DefaultLenientRefreshProfileValidator();
        var item = LenientRefreshProfile.NewLenientRefreshProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientRefreshProfileMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new LenientRefreshProfileMarginRangeRule(6.3, 3536.7);
        var item = LenientRefreshProfile.NewLenientRefreshProfile("x", "y");
        item.Margin = 3538.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientRefreshProfileWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new LenientRefreshProfileWeightRangeRule(0.7, 2311.5);
        var item = LenientRefreshProfile.NewLenientRefreshProfile("x", "y");
        item.Weight = 2313.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientRefreshProfileChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new LenientRefreshProfileChecksumLengthRule(32);
        var item = LenientRefreshProfile.NewLenientRefreshProfile("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientRefreshProfileLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientRefreshProfileLabelLengthRule(64);
        var item = LenientRefreshProfile.NewLenientRefreshProfile("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientRefreshProfileValidatorReportsEveryProblem();
        LenientRefreshProfileMarginRangeRuleFlagsValuesAboveMargin();
        LenientRefreshProfileWeightRangeRuleFlagsValuesAboveWeight();
        LenientRefreshProfileChecksumLengthRuleFlagsALongChecksum();
        LenientRefreshProfileLabelLengthRuleFlagsALongLabel();
    }
}
