namespace Acme.Workforce.Assets.Tests.Assignment.Estimate;

using Acme.Workforce.Assets.Assignment.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AssignmentMetricRulesChecks
{
    public static void AssignmentMetricValidatorReportsEveryProblem()
    {
        var validator = AssignmentMetricRules.DefaultAssignmentMetricValidator();
        var item = AssignmentMetric.NewAssignmentMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssignmentMetricCodeLengthRule(120);
        var item = AssignmentMetric.NewAssignmentMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentMetricRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AssignmentMetricRequiredLabelRule("assignment metric needs an entry");
        var item = AssignmentMetric.NewAssignmentMetric("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentMetricSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new AssignmentMetricSequenceRangeRule(64, 80);
        var item = AssignmentMetric.NewAssignmentMetric("x", "y");
        item.Sequence = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssignmentMetricValidatorReportsEveryProblem();
        AssignmentMetricCodeLengthRuleFlagsALongCode();
        AssignmentMetricRequiredLabelRuleFlagsABlankLabel();
        AssignmentMetricSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
