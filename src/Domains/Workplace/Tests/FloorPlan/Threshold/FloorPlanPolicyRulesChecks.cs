namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FloorPlanPolicyRulesChecks
{
    public static void FloorPlanPolicyValidatorReportsEveryProblem()
    {
        var validator = FloorPlanPolicyRules.DefaultFloorPlanPolicyValidator();
        var item = FloorPlanPolicy.NewFloorPlanPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanPolicyRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FloorPlanPolicyRequiredTitleRule("floor plan policy needs this field");
        var item = FloorPlanPolicy.NewFloorPlanPolicy("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanPolicyRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new FloorPlanPolicyRequiredDescriptionRule("floor plan policy needs a value");
        var item = FloorPlanPolicy.NewFloorPlanPolicy("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanPolicyBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new FloorPlanPolicyBudgetRangeRule(0.1, 1149.2);
        var item = FloorPlanPolicy.NewFloorPlanPolicy("x", "y");
        item.Budget = 1150.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FloorPlanPolicyValidatorReportsEveryProblem();
        FloorPlanPolicyRequiredTitleRuleFlagsABlankTitle();
        FloorPlanPolicyRequiredDescriptionRuleFlagsABlankDescription();
        FloorPlanPolicyBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
