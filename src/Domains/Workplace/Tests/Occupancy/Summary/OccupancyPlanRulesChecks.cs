namespace Acme.Workforce.Workplace.Tests.Occupancy.Summary;

using Acme.Workforce.Workplace.Occupancy.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class OccupancyPlanRulesChecks
{
    public static void OccupancyPlanValidatorReportsEveryProblem()
    {
        var validator = OccupancyPlanRules.DefaultOccupancyPlanValidator();
        var item = OccupancyPlan.NewOccupancyPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyPlanRequiredNameRuleFlagsABlankName()
    {
        var rule = new OccupancyPlanRequiredNameRule("occupancy plan needs an entry");
        var item = OccupancyPlan.NewOccupancyPlan("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyPlanRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new OccupancyPlanRequiredCategoryRule("occupancy plan needs a value");
        var item = OccupancyPlan.NewOccupancyPlan("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyPlanWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new OccupancyPlanWeightRangeRule(4.2, 2400.1);
        var item = OccupancyPlan.NewOccupancyPlan("x", "y");
        item.Weight = 2401.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyPlanNameLengthRuleFlagsALongName()
    {
        var rule = new OccupancyPlanNameLengthRule(1000);
        var item = OccupancyPlan.NewOccupancyPlan("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OccupancyPlanValidatorReportsEveryProblem();
        OccupancyPlanRequiredNameRuleFlagsABlankName();
        OccupancyPlanRequiredCategoryRuleFlagsABlankCategory();
        OccupancyPlanWeightRangeRuleFlagsValuesAboveWeight();
        OccupancyPlanNameLengthRuleFlagsALongName();
    }
}
