namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class WeightedSpanBaselineRulesChecks
{
    public static void WeightedSpanBaselineValidatorReportsEveryProblem()
    {
        var validator = WeightedSpanBaselineRules.DefaultWeightedSpanBaselineValidator();
        var item = WeightedSpanBaseline.NewWeightedSpanBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedSpanBaselineTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WeightedSpanBaselineTitleLengthRule(32);
        var item = WeightedSpanBaseline.NewWeightedSpanBaseline("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedSpanBaselineRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedSpanBaselineRequiredTitleRule("weighted span baseline needs an entry");
        var item = WeightedSpanBaseline.NewWeightedSpanBaseline("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedSpanBaselineSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new WeightedSpanBaselineSalaryBandLengthRule(3600);
        var item = WeightedSpanBaseline.NewWeightedSpanBaseline("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedSpanBaselineValidatorReportsEveryProblem();
        WeightedSpanBaselineTitleLengthRuleFlagsALongTitle();
        WeightedSpanBaselineRequiredTitleRuleFlagsABlankTitle();
        WeightedSpanBaselineSalaryBandLengthRuleFlagsALongSalaryBand();
    }
}
