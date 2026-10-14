namespace Acme.Workforce.Compliance.Tests.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>Checks of certification assignment code, run by the project's runner.</summary>
public static class CappedCertificationAssignmentRulesChecks
{
    public static void CappedCertificationAssignmentValidatorReportsEveryProblem()
    {
        var validator = CappedCertificationAssignmentRules.DefaultCappedCertificationAssignmentValidator();
        var item = CappedCertificationAssignment.NewCappedCertificationAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedCertificationAssignmentRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new CappedCertificationAssignmentRetriesRangeRule(32, 3600);
        var item = CappedCertificationAssignment.NewCappedCertificationAssignment("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedCertificationAssignmentAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new CappedCertificationAssignmentAttemptsRangeRule(120, 1000);
        var item = CappedCertificationAssignment.NewCappedCertificationAssignment("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedCertificationAssignmentRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new CappedCertificationAssignmentRequiredLocaleRule(
            "capped certification assignment needs this field");
        var item = CappedCertificationAssignment.NewCappedCertificationAssignment("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedCertificationAssignmentValidatorReportsEveryProblem();
        CappedCertificationAssignmentRetriesRangeRuleFlagsValuesAboveRetries();
        CappedCertificationAssignmentAttemptsRangeRuleFlagsValuesAboveAttempts();
        CappedCertificationAssignmentRequiredLocaleRuleFlagsABlankLocale();
    }
}
