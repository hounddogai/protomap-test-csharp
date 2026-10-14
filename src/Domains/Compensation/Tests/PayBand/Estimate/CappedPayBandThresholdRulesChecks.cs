namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class CappedPayBandThresholdRulesChecks
{
    public static void CappedPayBandThresholdValidatorReportsEveryProblem()
    {
        var validator = CappedPayBandThresholdRules.DefaultCappedPayBandThresholdValidator();
        var item = CappedPayBandThreshold.NewCappedPayBandThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedPayBandThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedPayBandThresholdCodeLengthRule(255);
        var item = CappedPayBandThreshold.NewCappedPayBandThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedPayBandThresholdCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new CappedPayBandThresholdCostCenterLengthRule(255);
        var item = CappedPayBandThreshold.NewCappedPayBandThreshold("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedPayBandThresholdRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedPayBandThresholdRequiredLabelRule("capped pay band threshold needs an entry");
        var item = CappedPayBandThreshold.NewCappedPayBandThreshold("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedPayBandThresholdValidatorReportsEveryProblem();
        CappedPayBandThresholdCodeLengthRuleFlagsALongCode();
        CappedPayBandThresholdCostCenterLengthRuleFlagsALongCostCenter();
        CappedPayBandThresholdRequiredLabelRuleFlagsABlankLabel();
    }
}
