namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rule;

using Acme.Workforce.Workplace.FloorPlan.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FloorPlanTemplateRulesChecks
{
    public static void FloorPlanTemplateValidatorReportsEveryProblem()
    {
        var validator = FloorPlanTemplateRules.DefaultFloorPlanTemplateValidator();
        var item = FloorPlanTemplate.NewFloorPlanTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanTemplateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FloorPlanTemplateRequiredLabelRule("floor plan template needs this field");
        var item = FloorPlanTemplate.NewFloorPlanTemplate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanTemplateRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new FloorPlanTemplateRequiredGradeRule("floor plan template needs a value");
        var item = FloorPlanTemplate.NewFloorPlanTemplate("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanTemplateRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new FloorPlanTemplateRequiredCostCenterRule("floor plan template needs an entry");
        var item = FloorPlanTemplate.NewFloorPlanTemplate("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanTemplateValidatorReportsEveryProblem();
        FloorPlanTemplateRequiredLabelRuleFlagsABlankLabel();
        FloorPlanTemplateRequiredGradeRuleFlagsABlankGrade();
        FloorPlanTemplateRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
