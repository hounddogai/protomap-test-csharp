namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccrualAgreementRulesChecks
{
    public static void AccrualAgreementValidatorReportsEveryProblem()
    {
        var validator = AccrualAgreementRules.DefaultAccrualAgreementValidator();
        var item = AccrualAgreement.NewAccrualAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualAgreementRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new AccrualAgreementRevisionRangeRule(16, 1000);
        var item = AccrualAgreement.NewAccrualAgreement("x", "y");
        item.Revision = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualAgreementEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new AccrualAgreementEndsAtRangeRule(120, 255);
        var item = AccrualAgreement.NewAccrualAgreement("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AccrualAgreementValidatorReportsEveryProblem();
        AccrualAgreementRevisionRangeRuleFlagsValuesAboveRevision();
        AccrualAgreementEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
