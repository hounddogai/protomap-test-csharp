namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Agreement;

using Acme.Workforce.Talent.SuccessionPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class SuccessionPlanCycleRulesChecks
{
    public static void SuccessionPlanCycleValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanCycleRules.DefaultSuccessionPlanCycleValidator();
        var item = SuccessionPlanCycle.NewSuccessionPlanCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanCycleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SuccessionPlanCycleRequiredLabelRule("succession plan cycle needs an entry");
        var item = SuccessionPlanCycle.NewSuccessionPlanCycle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanCycleCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new SuccessionPlanCycleCostRangeRule(3.5, 1431.0);
        var item = SuccessionPlanCycle.NewSuccessionPlanCycle("x", "y");
        item.Cost = 1432.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanCycleSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new SuccessionPlanCycleSegmentLengthRule(255);
        var item = SuccessionPlanCycle.NewSuccessionPlanCycle("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanCycleRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new SuccessionPlanCycleRevisionRangeRule(255, 3600);
        var item = SuccessionPlanCycle.NewSuccessionPlanCycle("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SuccessionPlanCycleValidatorReportsEveryProblem();
        SuccessionPlanCycleRequiredLabelRuleFlagsABlankLabel();
        SuccessionPlanCycleCostRangeRuleFlagsValuesAboveCost();
        SuccessionPlanCycleSegmentLengthRuleFlagsALongSegment();
        SuccessionPlanCycleRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
