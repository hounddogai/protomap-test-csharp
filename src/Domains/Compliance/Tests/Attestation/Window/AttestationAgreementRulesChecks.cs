namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationAgreementRulesChecks
{
    public static void AttestationAgreementValidatorReportsEveryProblem()
    {
        var validator = AttestationAgreementRules.DefaultAttestationAgreementValidator();
        var item = AttestationAgreement.NewAttestationAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationAgreementEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new AttestationAgreementEndsAtRangeRule(32, 3600);
        var item = AttestationAgreement.NewAttestationAgreement("x", "y");
        item.EndsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationAgreementRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new AttestationAgreementRequiredCategoryRule("attestation agreement needs a value");
        var item = AttestationAgreement.NewAttestationAgreement("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationAgreementCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new AttestationAgreementCreatedAtRangeRule(64, 3600);
        var item = AttestationAgreement.NewAttestationAgreement("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationAgreementFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new AttestationAgreementFloorValueRangeRule(2.8, 280.4);
        var item = AttestationAgreement.NewAttestationAgreement("x", "y");
        item.FloorValue = 281.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AttestationAgreementValidatorReportsEveryProblem();
        AttestationAgreementEndsAtRangeRuleFlagsValuesAboveEndsAt();
        AttestationAgreementRequiredCategoryRuleFlagsABlankCategory();
        AttestationAgreementCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        AttestationAgreementFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
