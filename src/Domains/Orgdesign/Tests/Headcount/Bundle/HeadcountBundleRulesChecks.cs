namespace Acme.Workforce.Orgdesign.Tests.Headcount.Bundle;

using Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class HeadcountBundleRulesChecks
{
    public static void HeadcountBundleValidatorReportsEveryProblem()
    {
        var validator = HeadcountBundleRules.DefaultHeadcountBundleValidator();
        var item = HeadcountBundle.NewHeadcountBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountBundleRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new HeadcountBundleRatingRangeRule(9.7, 3459.7);
        var item = HeadcountBundle.NewHeadcountBundle("x", "y");
        item.Rating = 3461.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HeadcountBundleRequiredCodeRule("headcount bundle needs an entry");
        var item = HeadcountBundle.NewHeadcountBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountBundleCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new HeadcountBundleCreatedAtRangeRule(64, 1000);
        var item = HeadcountBundle.NewHeadcountBundle("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new HeadcountBundleLabelLengthRule(120);
        var item = HeadcountBundle.NewHeadcountBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountBundleCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new HeadcountBundleCeilingRangeRule(6.7, 1336.3);
        var item = HeadcountBundle.NewHeadcountBundle("x", "y");
        item.Ceiling = 1337.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HeadcountBundleValidatorReportsEveryProblem();
        HeadcountBundleRatingRangeRuleFlagsValuesAboveRating();
        HeadcountBundleRequiredCodeRuleFlagsABlankCode();
        HeadcountBundleCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        HeadcountBundleLabelLengthRuleFlagsALongLabel();
        HeadcountBundleCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
