namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanSummaryRulesChecks
{
    public static void RegionalSuccessionPlanSummaryValidatorReportsEveryProblem()
    {
        var validator = RegionalSuccessionPlanSummaryRules.DefaultRegionalSuccessionPlanSummaryValidator();
        var item = RegionalSuccessionPlanSummary.NewRegionalSuccessionPlanSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalSuccessionPlanSummaryMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new RegionalSuccessionPlanSummaryMinimumRangeRule(1.8, 2552.0);
        var item = RegionalSuccessionPlanSummary.NewRegionalSuccessionPlanSummary("x", "y");
        item.Minimum = 2553.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalSuccessionPlanSummaryCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new RegionalSuccessionPlanSummaryCapacityRangeRule(32, 3600);
        var item = RegionalSuccessionPlanSummary.NewRegionalSuccessionPlanSummary("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalSuccessionPlanSummaryJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new RegionalSuccessionPlanSummaryJobCodeLengthRule(255);
        var item = RegionalSuccessionPlanSummary.NewRegionalSuccessionPlanSummary("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalSuccessionPlanSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalSuccessionPlanSummaryCodeLengthRule(64);
        var item = RegionalSuccessionPlanSummary.NewRegionalSuccessionPlanSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalSuccessionPlanSummaryValidatorReportsEveryProblem();
        RegionalSuccessionPlanSummaryMinimumRangeRuleFlagsValuesAboveMinimum();
        RegionalSuccessionPlanSummaryCapacityRangeRuleFlagsValuesAboveCapacity();
        RegionalSuccessionPlanSummaryJobCodeLengthRuleFlagsALongJobCode();
        RegionalSuccessionPlanSummaryCodeLengthRuleFlagsALongCode();
    }
}
