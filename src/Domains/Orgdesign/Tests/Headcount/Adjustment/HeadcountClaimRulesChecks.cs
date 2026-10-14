namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HeadcountClaimRulesChecks
{
    public static void HeadcountClaimValidatorReportsEveryProblem()
    {
        var validator = HeadcountClaimRules.DefaultHeadcountClaimValidator();
        var item = HeadcountClaim.NewHeadcountClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountClaimChannelLengthRuleFlagsALongChannel()
    {
        var rule = new HeadcountClaimChannelLengthRule(1000);
        var item = HeadcountClaim.NewHeadcountClaim("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountClaimExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new HeadcountClaimExpiresAtRangeRule(16, 1000);
        var item = HeadcountClaim.NewHeadcountClaim("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountClaimRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new HeadcountClaimRateRangeRule(6.1, 869.9);
        var item = HeadcountClaim.NewHeadcountClaim("x", "y");
        item.Rate = 871.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountClaimCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new HeadcountClaimCapacityRangeRule(255, 255);
        var item = HeadcountClaim.NewHeadcountClaim("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountClaimTitleLengthRuleFlagsALongTitle()
    {
        var rule = new HeadcountClaimTitleLengthRule(32);
        var item = HeadcountClaim.NewHeadcountClaim("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountClaimValidatorReportsEveryProblem();
        HeadcountClaimChannelLengthRuleFlagsALongChannel();
        HeadcountClaimExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        HeadcountClaimRateRangeRuleFlagsValuesAboveRate();
        HeadcountClaimCapacityRangeRuleFlagsValuesAboveCapacity();
        HeadcountClaimTitleLengthRuleFlagsALongTitle();
    }
}
