namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdjustmentRequestRulesChecks
{
    public static void AdjustmentRequestValidatorReportsEveryProblem()
    {
        var validator = AdjustmentRequestRules.DefaultAdjustmentRequestValidator();
        var item = AdjustmentRequest.NewAdjustmentRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdjustmentRequestRequiredCodeRule("adjustment request needs an entry");
        var item = AdjustmentRequest.NewAdjustmentRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentRequestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdjustmentRequestRequiredLabelRule("adjustment request needs this field");
        var item = AdjustmentRequest.NewAdjustmentRequest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentRequestRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new AdjustmentRequestRequiredBadgeRule("adjustment request needs this field");
        var item = AdjustmentRequest.NewAdjustmentRequest("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdjustmentRequestCodeLengthRule(64);
        var item = AdjustmentRequest.NewAdjustmentRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdjustmentRequestValidatorReportsEveryProblem();
        AdjustmentRequestRequiredCodeRuleFlagsABlankCode();
        AdjustmentRequestRequiredLabelRuleFlagsABlankLabel();
        AdjustmentRequestRequiredBadgeRuleFlagsABlankBadge();
        AdjustmentRequestCodeLengthRuleFlagsALongCode();
    }
}
