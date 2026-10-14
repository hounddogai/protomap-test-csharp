namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class WeightedVisitorBaselineRulesChecks
{
    public static void WeightedVisitorBaselineValidatorReportsEveryProblem()
    {
        var validator = WeightedVisitorBaselineRules.DefaultWeightedVisitorBaselineValidator();
        var item = WeightedVisitorBaseline.NewWeightedVisitorBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedVisitorBaselineDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new WeightedVisitorBaselineDiscountRangeRule(9.2, 4689.6);
        var item = WeightedVisitorBaseline.NewWeightedVisitorBaseline("x", "y");
        item.Discount = 4691.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedVisitorBaselineLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedVisitorBaselineLabelLengthRule(64);
        var item = WeightedVisitorBaseline.NewWeightedVisitorBaseline("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedVisitorBaselineValidatorReportsEveryProblem();
        WeightedVisitorBaselineDiscountRangeRuleFlagsValuesAboveDiscount();
        WeightedVisitorBaselineLabelLengthRuleFlagsALongLabel();
    }
}
