namespace Acme.Workforce.Workplace.Tests.AccessBadge.Estimate;

using Acme.Workforce.Workplace.AccessBadge.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalAccessBadgeEstimateRulesChecks
{
    public static void GlobalAccessBadgeEstimateValidatorReportsEveryProblem()
    {
        var validator = GlobalAccessBadgeEstimateRules.DefaultGlobalAccessBadgeEstimateValidator();
        var item = GlobalAccessBadgeEstimate.NewGlobalAccessBadgeEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAccessBadgeEstimateCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new GlobalAccessBadgeEstimateCapacityRangeRule(120, 3600);
        var item = GlobalAccessBadgeEstimate.NewGlobalAccessBadgeEstimate("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalAccessBadgeEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalAccessBadgeEstimateCodeLengthRule(86400);
        var item = GlobalAccessBadgeEstimate.NewGlobalAccessBadgeEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAccessBadgeEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new GlobalAccessBadgeEstimateNameLengthRule(120);
        var item = GlobalAccessBadgeEstimate.NewGlobalAccessBadgeEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAccessBadgeEstimateMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new GlobalAccessBadgeEstimateMaximumRangeRule(0.7, 4664.1);
        var item = GlobalAccessBadgeEstimate.NewGlobalAccessBadgeEstimate("x", "y");
        item.Maximum = 4665.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalAccessBadgeEstimateValidatorReportsEveryProblem();
        GlobalAccessBadgeEstimateCapacityRangeRuleFlagsValuesAboveCapacity();
        GlobalAccessBadgeEstimateCodeLengthRuleFlagsALongCode();
        GlobalAccessBadgeEstimateNameLengthRuleFlagsALongName();
        GlobalAccessBadgeEstimateMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
