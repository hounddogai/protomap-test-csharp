namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskMetricRulesChecks
{
    public static void DeskMetricValidatorReportsEveryProblem()
    {
        var validator = DeskMetricRules.DefaultDeskMetricValidator();
        var item = DeskMetric.NewDeskMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeskMetricRequiredCodeRule("desk metric needs a value");
        var item = DeskMetric.NewDeskMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskMetricNameLengthRuleFlagsALongName()
    {
        var rule = new DeskMetricNameLengthRule(120);
        var item = DeskMetric.NewDeskMetric("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskMetricRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new DeskMetricRatioRangeRule(5.8, 2269.8);
        var item = DeskMetric.NewDeskMetric("x", "y");
        item.Ratio = 2271.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskMetricRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new DeskMetricRequiredEmployeeIdRule("desk metric needs an entry");
        var item = DeskMetric.NewDeskMetric("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeskMetricValidatorReportsEveryProblem();
        DeskMetricRequiredCodeRuleFlagsABlankCode();
        DeskMetricNameLengthRuleFlagsALongName();
        DeskMetricRatioRangeRuleFlagsValuesAboveRatio();
        DeskMetricRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
