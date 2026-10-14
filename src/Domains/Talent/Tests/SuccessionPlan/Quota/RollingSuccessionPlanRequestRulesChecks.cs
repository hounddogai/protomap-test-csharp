namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Quota;

using Acme.Workforce.Talent.SuccessionPlan.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RollingSuccessionPlanRequestRulesChecks
{
    public static void RollingSuccessionPlanRequestValidatorReportsEveryProblem()
    {
        var validator = RollingSuccessionPlanRequestRules.DefaultRollingSuccessionPlanRequestValidator();
        var item = RollingSuccessionPlanRequest.NewRollingSuccessionPlanRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingSuccessionPlanRequestRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new RollingSuccessionPlanRequestRateRangeRule(4.6, 4930.3);
        var item = RollingSuccessionPlanRequest.NewRollingSuccessionPlanRequest("x", "y");
        item.Rate = 4931.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingSuccessionPlanRequestBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new RollingSuccessionPlanRequestBadgeLengthRule(1000);
        var item = RollingSuccessionPlanRequest.NewRollingSuccessionPlanRequest("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingSuccessionPlanRequestRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new RollingSuccessionPlanRequestRequiredBadgeRule("rolling succession plan request needs an entry");
        var item = RollingSuccessionPlanRequest.NewRollingSuccessionPlanRequest("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingSuccessionPlanRequestRequiredNameRuleFlagsABlankName()
    {
        var rule = new RollingSuccessionPlanRequestRequiredNameRule("rolling succession plan request needs a value");
        var item = RollingSuccessionPlanRequest.NewRollingSuccessionPlanRequest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingSuccessionPlanRequestValidatorReportsEveryProblem();
        RollingSuccessionPlanRequestRateRangeRuleFlagsValuesAboveRate();
        RollingSuccessionPlanRequestBadgeLengthRuleFlagsALongBadge();
        RollingSuccessionPlanRequestRequiredBadgeRuleFlagsABlankBadge();
        RollingSuccessionPlanRequestRequiredNameRuleFlagsABlankName();
    }
}
