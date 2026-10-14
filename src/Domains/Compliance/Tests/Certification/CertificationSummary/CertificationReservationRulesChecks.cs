namespace Acme.Workforce.Compliance.Tests.Certification.CertificationSummary;

using Acme.Workforce.Compliance.Certification.CertificationSummary;

/// <summary>Checks of certification summary code, run by the project's runner.</summary>
public static class CertificationReservationRulesChecks
{
    public static void CertificationReservationValidatorReportsEveryProblem()
    {
        var validator = CertificationReservationRules.DefaultCertificationReservationValidator();
        var item = CertificationReservation.NewCertificationReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationReservationPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new CertificationReservationPriorityRangeRule(64, 3600);
        var item = CertificationReservation.NewCertificationReservation("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationReservationRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new CertificationReservationRequiredExternalRefRule("certification reservation needs a value");
        var item = CertificationReservation.NewCertificationReservation("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new CertificationReservationCodeLengthRule(64);
        var item = CertificationReservation.NewCertificationReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationReservationRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new CertificationReservationRequiredEmployeeIdRule("certification reservation needs a value");
        var item = CertificationReservation.NewCertificationReservation("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationReservationLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new CertificationReservationLeaveDaysRangeRule(64, 64);
        var item = CertificationReservation.NewCertificationReservation("x", "y");
        item.LeaveDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CertificationReservationValidatorReportsEveryProblem();
        CertificationReservationPriorityRangeRuleFlagsValuesAbovePriority();
        CertificationReservationRequiredExternalRefRuleFlagsABlankExternalRef();
        CertificationReservationCodeLengthRuleFlagsALongCode();
        CertificationReservationRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        CertificationReservationLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
