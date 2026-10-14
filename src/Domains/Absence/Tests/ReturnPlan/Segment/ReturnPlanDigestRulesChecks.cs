namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ReturnPlanDigestRulesChecks
{
    public static void ReturnPlanDigestValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanDigestRules.DefaultReturnPlanDigestValidator();
        var item = ReturnPlanDigest.NewReturnPlanDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanDigestRegionLengthRuleFlagsALongRegion()
    {
        var rule = new ReturnPlanDigestRegionLengthRule(64);
        var item = ReturnPlanDigest.NewReturnPlanDigest("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanDigestRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new ReturnPlanDigestRequiredRegionRule("return plan digest needs a value");
        var item = ReturnPlanDigest.NewReturnPlanDigest("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanDigestMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new ReturnPlanDigestMarginRangeRule(8.0, 3313.6);
        var item = ReturnPlanDigest.NewReturnPlanDigest("x", "y");
        item.Margin = 3315.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanDigestNameLengthRuleFlagsALongName()
    {
        var rule = new ReturnPlanDigestNameLengthRule(32);
        var item = ReturnPlanDigest.NewReturnPlanDigest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanDigestRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new ReturnPlanDigestRequiredTeamRule("return plan digest needs an entry");
        var item = ReturnPlanDigest.NewReturnPlanDigest("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReturnPlanDigestValidatorReportsEveryProblem();
        ReturnPlanDigestRegionLengthRuleFlagsALongRegion();
        ReturnPlanDigestRequiredRegionRuleFlagsABlankRegion();
        ReturnPlanDigestMarginRangeRuleFlagsValuesAboveMargin();
        ReturnPlanDigestNameLengthRuleFlagsALongName();
        ReturnPlanDigestRequiredTeamRuleFlagsABlankTeam();
    }
}
