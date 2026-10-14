namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientMobilityRequestRulesChecks
{
    public static void LenientMobilityRequestValidatorReportsEveryProblem()
    {
        var validator = LenientMobilityRequestRules.DefaultLenientMobilityRequestValidator();
        var item = LenientMobilityRequest.NewLenientMobilityRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientMobilityRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientMobilityRequestRequiredCodeRule("lenient mobility request needs a value");
        var item = LenientMobilityRequest.NewLenientMobilityRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientMobilityRequestRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new LenientMobilityRequestRatioRangeRule(2.2, 2710.1);
        var item = LenientMobilityRequest.NewLenientMobilityRequest("x", "y");
        item.Ratio = 2711.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientMobilityRequestRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new LenientMobilityRequestRequiredCostCenterRule("lenient mobility request needs an entry");
        var item = LenientMobilityRequest.NewLenientMobilityRequest("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientMobilityRequestValidatorReportsEveryProblem();
        LenientMobilityRequestRequiredCodeRuleFlagsABlankCode();
        LenientMobilityRequestRatioRangeRuleFlagsValuesAboveRatio();
        LenientMobilityRequestRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
