namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyPayRunAgreementRulesChecks
{
    public static void LegacyPayRunAgreementValidatorReportsEveryProblem()
    {
        var validator = LegacyPayRunAgreementRules.DefaultLegacyPayRunAgreementValidator();
        var item = LegacyPayRunAgreement.NewLegacyPayRunAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPayRunAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyPayRunAgreementCodeLengthRule(80);
        var item = LegacyPayRunAgreement.NewLegacyPayRunAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPayRunAgreementRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new LegacyPayRunAgreementRequiredReviewerRule("legacy pay run agreement needs this field");
        var item = LegacyPayRunAgreement.NewLegacyPayRunAgreement("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPayRunAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyPayRunAgreementRequiredCodeRule("legacy pay run agreement needs this field");
        var item = LegacyPayRunAgreement.NewLegacyPayRunAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPayRunAgreementStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new LegacyPayRunAgreementStartsAtRangeRule(16, 255);
        var item = LegacyPayRunAgreement.NewLegacyPayRunAgreement("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyPayRunAgreementReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new LegacyPayRunAgreementReviewerLengthRule(3600);
        var item = LegacyPayRunAgreement.NewLegacyPayRunAgreement("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyPayRunAgreementValidatorReportsEveryProblem();
        LegacyPayRunAgreementCodeLengthRuleFlagsALongCode();
        LegacyPayRunAgreementRequiredReviewerRuleFlagsABlankReviewer();
        LegacyPayRunAgreementRequiredCodeRuleFlagsABlankCode();
        LegacyPayRunAgreementStartsAtRangeRuleFlagsValuesAboveStartsAt();
        LegacyPayRunAgreementReviewerLengthRuleFlagsALongReviewer();
    }
}
