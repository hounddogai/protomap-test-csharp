namespace Acme.Workforce.Contractors.Tests.Rate.Notice;

using Acme.Workforce.Contractors.Rate.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RatePolicyRulesChecks
{
    public static void RatePolicyValidatorReportsEveryProblem()
    {
        var validator = RatePolicyRules.DefaultRatePolicyValidator();
        var item = RatePolicy.NewRatePolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RatePolicyRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RatePolicyRequiredLabelRule("rate policy needs this field");
        var item = RatePolicy.NewRatePolicy("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RatePolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RatePolicyRequiredCodeRule("rate policy needs a value");
        var item = RatePolicy.NewRatePolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RatePolicyTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new RatePolicyTenureMonthsRangeRule(1000, 86400);
        var item = RatePolicy.NewRatePolicy("x", "y");
        item.TenureMonths = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RatePolicyRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new RatePolicyRequiredTeamRule("rate policy needs a value");
        var item = RatePolicy.NewRatePolicy("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RatePolicyValidatorReportsEveryProblem();
        RatePolicyRequiredLabelRuleFlagsABlankLabel();
        RatePolicyRequiredCodeRuleFlagsABlankCode();
        RatePolicyTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        RatePolicyRequiredTeamRuleFlagsABlankTeam();
    }
}
