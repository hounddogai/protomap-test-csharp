namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CareerPathMetricRulesChecks
{
    public static void CareerPathMetricValidatorReportsEveryProblem()
    {
        var validator = CareerPathMetricRules.DefaultCareerPathMetricValidator();
        var item = CareerPathMetric.NewCareerPathMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathMetricRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new CareerPathMetricRequiredNotesRule("career path metric needs an entry");
        var item = CareerPathMetric.NewCareerPathMetric("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathMetricTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new CareerPathMetricTimezoneLengthRule(64);
        var item = CareerPathMetric.NewCareerPathMetric("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new CareerPathMetricCodeLengthRule(86400);
        var item = CareerPathMetric.NewCareerPathMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathMetricSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new CareerPathMetricSequenceRangeRule(120, 1000);
        var item = CareerPathMetric.NewCareerPathMetric("x", "y");
        item.Sequence = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathMetricNotesLengthRuleFlagsALongNotes()
    {
        var rule = new CareerPathMetricNotesLengthRule(86400);
        var item = CareerPathMetric.NewCareerPathMetric("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CareerPathMetricValidatorReportsEveryProblem();
        CareerPathMetricRequiredNotesRuleFlagsABlankNotes();
        CareerPathMetricTimezoneLengthRuleFlagsALongTimezone();
        CareerPathMetricCodeLengthRuleFlagsALongCode();
        CareerPathMetricSequenceRangeRuleFlagsValuesAboveSequence();
        CareerPathMetricNotesLengthRuleFlagsALongNotes();
    }
}
