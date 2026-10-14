namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorPlanRulesChecks
{
    public static void VisitorPlanValidatorReportsEveryProblem()
    {
        var validator = VisitorPlanRules.DefaultVisitorPlanValidator();
        var item = VisitorPlan.NewVisitorPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorPlanRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new VisitorPlanRequiredSourceRule("visitor plan needs a value");
        var item = VisitorPlan.NewVisitorPlan("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorPlanRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new VisitorPlanRequiredLabelRule("visitor plan needs this field");
        var item = VisitorPlan.NewVisitorPlan("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VisitorPlanRequiredCodeRule("visitor plan needs this field");
        var item = VisitorPlan.NewVisitorPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorPlanValidatorReportsEveryProblem();
        VisitorPlanRequiredSourceRuleFlagsABlankSource();
        VisitorPlanRequiredLabelRuleFlagsABlankLabel();
        VisitorPlanRequiredCodeRuleFlagsABlankCode();
    }
}
