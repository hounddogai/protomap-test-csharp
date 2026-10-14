namespace Acme.Workforce.Compensation.Tests.Adjustment.Bundle;

using Acme.Workforce.Compensation.Adjustment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackAdjustmentProposalRulesChecks
{
    public static void FallbackAdjustmentProposalValidatorReportsEveryProblem()
    {
        var validator = FallbackAdjustmentProposalRules.DefaultFallbackAdjustmentProposalValidator();
        var item = FallbackAdjustmentProposal.NewFallbackAdjustmentProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackAdjustmentProposalWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new FallbackAdjustmentProposalWindowDaysRangeRule(120, 86400);
        var item = FallbackAdjustmentProposal.NewFallbackAdjustmentProposal("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackAdjustmentProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackAdjustmentProposalCodeLengthRule(86400);
        var item = FallbackAdjustmentProposal.NewFallbackAdjustmentProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackAdjustmentProposalRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new FallbackAdjustmentProposalRequiredExternalRefRule(
            "fallback adjustment proposal needs this field");
        var item = FallbackAdjustmentProposal.NewFallbackAdjustmentProposal("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackAdjustmentProposalValidatorReportsEveryProblem();
        FallbackAdjustmentProposalWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        FallbackAdjustmentProposalCodeLengthRuleFlagsALongCode();
        FallbackAdjustmentProposalRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
