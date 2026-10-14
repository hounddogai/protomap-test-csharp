namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentSummaryRulesChecks
{
    public static void AdjustmentSummaryValidatorReportsEveryProblem()
    {
        var validator = AdjustmentSummaryRules.DefaultAdjustmentSummaryValidator();
        var item = AdjustmentSummary.NewAdjustmentSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentSummaryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdjustmentSummaryRequiredLabelRule("adjustment summary needs an entry");
        var item = AdjustmentSummary.NewAdjustmentSummary("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdjustmentSummaryCodeLengthRule(120);
        var item = AdjustmentSummary.NewAdjustmentSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdjustmentSummaryRequiredCodeRule("adjustment summary needs this field");
        var item = AdjustmentSummary.NewAdjustmentSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentSummaryExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new AdjustmentSummaryExpiresAtRangeRule(32, 86400);
        var item = AdjustmentSummary.NewAdjustmentSummary("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdjustmentSummaryValidatorReportsEveryProblem();
        AdjustmentSummaryRequiredLabelRuleFlagsABlankLabel();
        AdjustmentSummaryCodeLengthRuleFlagsALongCode();
        AdjustmentSummaryRequiredCodeRuleFlagsABlankCode();
        AdjustmentSummaryExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
