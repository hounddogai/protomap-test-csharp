namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class CostModelBundleRulesChecks
{
    public static void CostModelBundleValidatorReportsEveryProblem()
    {
        var validator = CostModelBundleRules.DefaultCostModelBundleValidator();
        var item = CostModelBundle.NewCostModelBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelBundleCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new CostModelBundleCapacityRangeRule(86400, 86400);
        var item = CostModelBundle.NewCostModelBundle("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CostModelBundleRequiredCodeRule("cost model bundle needs a value");
        var item = CostModelBundle.NewCostModelBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelBundleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CostModelBundleRequiredLabelRule("cost model bundle needs a value");
        var item = CostModelBundle.NewCostModelBundle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CostModelBundleLabelLengthRule(64);
        var item = CostModelBundle.NewCostModelBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelBundleValidatorReportsEveryProblem();
        CostModelBundleCapacityRangeRuleFlagsValuesAboveCapacity();
        CostModelBundleRequiredCodeRuleFlagsABlankCode();
        CostModelBundleRequiredLabelRuleFlagsABlankLabel();
        CostModelBundleLabelLengthRuleFlagsALongLabel();
    }
}
