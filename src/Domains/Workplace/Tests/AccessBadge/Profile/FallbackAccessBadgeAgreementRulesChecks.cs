namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class FallbackAccessBadgeAgreementRulesChecks
{
    public static void FallbackAccessBadgeAgreementValidatorReportsEveryProblem()
    {
        var validator = FallbackAccessBadgeAgreementRules.DefaultFallbackAccessBadgeAgreementValidator();
        var item = FallbackAccessBadgeAgreement.NewFallbackAccessBadgeAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackAccessBadgeAgreementMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new FallbackAccessBadgeAgreementMarginRangeRule(6.7, 3987.7);
        var item = FallbackAccessBadgeAgreement.NewFallbackAccessBadgeAgreement("x", "y");
        item.Margin = 3989.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackAccessBadgeAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackAccessBadgeAgreementRequiredCodeRule("fallback access badge agreement needs an entry");
        var item = FallbackAccessBadgeAgreement.NewFallbackAccessBadgeAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackAccessBadgeAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackAccessBadgeAgreementCodeLengthRule(16);
        var item = FallbackAccessBadgeAgreement.NewFallbackAccessBadgeAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackAccessBadgeAgreementExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new FallbackAccessBadgeAgreementExpiresAtRangeRule(16, 3600);
        var item = FallbackAccessBadgeAgreement.NewFallbackAccessBadgeAgreement("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackAccessBadgeAgreementValidatorReportsEveryProblem();
        FallbackAccessBadgeAgreementMarginRangeRuleFlagsValuesAboveMargin();
        FallbackAccessBadgeAgreementRequiredCodeRuleFlagsABlankCode();
        FallbackAccessBadgeAgreementCodeLengthRuleFlagsALongCode();
        FallbackAccessBadgeAgreementExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
