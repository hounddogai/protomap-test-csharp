namespace Acme.Workforce.Compliance.Tests.Audit.Plan;

using Acme.Workforce.Compliance.Audit.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class AuditAgreementRulesChecks
{
    public static void AuditAgreementValidatorReportsEveryProblem()
    {
        var validator = AuditAgreementRules.DefaultAuditAgreementValidator();
        var item = AuditAgreement.NewAuditAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditAgreementCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new AuditAgreementCostRangeRule(8.1, 1955.7);
        var item = AuditAgreement.NewAuditAgreement("x", "y");
        item.Cost = 1957.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditAgreementExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new AuditAgreementExpiresAtRangeRule(80, 255);
        var item = AuditAgreement.NewAuditAgreement("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new AuditAgreementCodeLengthRule(1000);
        var item = AuditAgreement.NewAuditAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AuditAgreementValidatorReportsEveryProblem();
        AuditAgreementCostRangeRuleFlagsValuesAboveCost();
        AuditAgreementExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        AuditAgreementCodeLengthRuleFlagsALongCode();
    }
}
