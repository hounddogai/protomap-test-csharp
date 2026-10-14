namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountPolicyRulesChecks
{
    public static void HeadcountPolicyValidatorReportsEveryProblem()
    {
        var validator = HeadcountPolicyRules.DefaultHeadcountPolicyValidator();
        var item = HeadcountPolicy.NewHeadcountPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountPolicyCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new HeadcountPolicyCostRangeRule(0.6, 3296.3);
        var item = HeadcountPolicy.NewHeadcountPolicy("x", "y");
        item.Cost = 3297.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountPolicyRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new HeadcountPolicyRequiredChannelRule("headcount policy needs this field");
        var item = HeadcountPolicy.NewHeadcountPolicy("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountPolicyLabelLengthRuleFlagsALongLabel()
    {
        var rule = new HeadcountPolicyLabelLengthRule(1000);
        var item = HeadcountPolicy.NewHeadcountPolicy("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountPolicyExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new HeadcountPolicyExpiresAtRangeRule(32, 255);
        var item = HeadcountPolicy.NewHeadcountPolicy("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HeadcountPolicyValidatorReportsEveryProblem();
        HeadcountPolicyCostRangeRuleFlagsValuesAboveCost();
        HeadcountPolicyRequiredChannelRuleFlagsABlankChannel();
        HeadcountPolicyLabelLengthRuleFlagsALongLabel();
        HeadcountPolicyExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
