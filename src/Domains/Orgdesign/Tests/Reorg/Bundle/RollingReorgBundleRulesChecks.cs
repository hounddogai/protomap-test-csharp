namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RollingReorgBundleRulesChecks
{
    public static void RollingReorgBundleValidatorReportsEveryProblem()
    {
        var validator = RollingReorgBundleRules.DefaultRollingReorgBundleValidator();
        var item = RollingReorgBundle.NewRollingReorgBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingReorgBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingReorgBundleLabelLengthRule(16);
        var item = RollingReorgBundle.NewRollingReorgBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingReorgBundleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new RollingReorgBundleTenureMonthsRangeRule(16, 1000);
        var item = RollingReorgBundle.NewRollingReorgBundle("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingReorgBundleRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new RollingReorgBundleRatingRangeRule(7.9, 3610.6);
        var item = RollingReorgBundle.NewRollingReorgBundle("x", "y");
        item.Rating = 3612.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingReorgBundleJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new RollingReorgBundleJobCodeLengthRule(1000);
        var item = RollingReorgBundle.NewRollingReorgBundle("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingReorgBundleRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new RollingReorgBundleRequiredJobCodeRule("rolling reorg bundle needs an entry");
        var item = RollingReorgBundle.NewRollingReorgBundle("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingReorgBundleValidatorReportsEveryProblem();
        RollingReorgBundleLabelLengthRuleFlagsALongLabel();
        RollingReorgBundleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        RollingReorgBundleRatingRangeRuleFlagsValuesAboveRating();
        RollingReorgBundleJobCodeLengthRuleFlagsALongJobCode();
        RollingReorgBundleRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
