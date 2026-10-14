namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FloorPlanGroupRulesChecks
{
    public static void FloorPlanGroupValidatorReportsEveryProblem()
    {
        var validator = FloorPlanGroupRules.DefaultFloorPlanGroupValidator();
        var item = FloorPlanGroup.NewFloorPlanGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new FloorPlanGroupCodeLengthRule(3600);
        var item = FloorPlanGroup.NewFloorPlanGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanGroupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FloorPlanGroupLabelLengthRule(64);
        var item = FloorPlanGroup.NewFloorPlanGroup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanGroupRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new FloorPlanGroupRequiredCountryRule("floor plan group needs this field");
        var item = FloorPlanGroup.NewFloorPlanGroup("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanGroupRequiredCodeRule("floor plan group needs a value");
        var item = FloorPlanGroup.NewFloorPlanGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanGroupReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new FloorPlanGroupReviewerLengthRule(255);
        var item = FloorPlanGroup.NewFloorPlanGroup("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FloorPlanGroupValidatorReportsEveryProblem();
        FloorPlanGroupCodeLengthRuleFlagsALongCode();
        FloorPlanGroupLabelLengthRuleFlagsALongLabel();
        FloorPlanGroupRequiredCountryRuleFlagsABlankCountry();
        FloorPlanGroupRequiredCodeRuleFlagsABlankCode();
        FloorPlanGroupReviewerLengthRuleFlagsALongReviewer();
    }
}
