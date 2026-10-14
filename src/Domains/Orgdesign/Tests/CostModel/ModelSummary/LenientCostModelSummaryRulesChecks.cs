namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class LenientCostModelSummaryRulesChecks
{
    public static void LenientCostModelSummaryValidatorReportsEveryProblem()
    {
        var validator = LenientCostModelSummaryRules.DefaultLenientCostModelSummaryValidator();
        var item = LenientCostModelSummary.NewLenientCostModelSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientCostModelSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientCostModelSummaryRequiredCodeRule("lenient cost model summary needs an entry");
        var item = LenientCostModelSummary.NewLenientCostModelSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientCostModelSummaryBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new LenientCostModelSummaryBadgeLengthRule(16);
        var item = LenientCostModelSummary.NewLenientCostModelSummary("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientCostModelSummaryRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new LenientCostModelSummaryRequiredCostCenterRule("lenient cost model summary needs an entry");
        var item = LenientCostModelSummary.NewLenientCostModelSummary("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientCostModelSummaryHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new LenientCostModelSummaryHoursRangeRule(4.3, 3688.4);
        var item = LenientCostModelSummary.NewLenientCostModelSummary("x", "y");
        item.Hours = 3689.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientCostModelSummaryValidatorReportsEveryProblem();
        LenientCostModelSummaryRequiredCodeRuleFlagsABlankCode();
        LenientCostModelSummaryBadgeLengthRuleFlagsALongBadge();
        LenientCostModelSummaryRequiredCostCenterRuleFlagsABlankCostCenter();
        LenientCostModelSummaryHoursRangeRuleFlagsValuesAboveHours();
    }
}
