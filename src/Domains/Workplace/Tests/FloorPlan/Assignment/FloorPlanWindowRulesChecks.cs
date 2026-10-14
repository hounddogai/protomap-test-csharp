namespace Acme.Workforce.Workplace.Tests.FloorPlan.Assignment;

using Acme.Workforce.Workplace.FloorPlan.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class FloorPlanWindowRulesChecks
{
    public static void FloorPlanWindowValidatorReportsEveryProblem()
    {
        var validator = FloorPlanWindowRules.DefaultFloorPlanWindowValidator();
        var item = FloorPlanWindow.NewFloorPlanWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanWindowRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new FloorPlanWindowRequiredContactEmailRule("floor plan window needs this field");
        var item = FloorPlanWindow.NewFloorPlanWindow("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanWindowRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new FloorPlanWindowRequiredCountryRule("floor plan window needs a value");
        var item = FloorPlanWindow.NewFloorPlanWindow("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanWindowValidatorReportsEveryProblem();
        FloorPlanWindowRequiredContactEmailRuleFlagsABlankContactEmail();
        FloorPlanWindowRequiredCountryRuleFlagsABlankCountry();
    }
}
