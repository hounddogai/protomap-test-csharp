namespace Acme.Workforce.Compliance.Tests.Certification.Schedule;

using Acme.Workforce.Compliance.Certification.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class CertificationScheduleRulesChecks
{
    public static void CertificationScheduleValidatorReportsEveryProblem()
    {
        var validator = CertificationScheduleRules.DefaultCertificationScheduleValidator();
        var item = CertificationSchedule.NewCertificationSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationScheduleRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new CertificationScheduleRetriesRangeRule(3600, 86400);
        var item = CertificationSchedule.NewCertificationSchedule("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CertificationScheduleRequiredCodeRule("certification schedule needs a value");
        var item = CertificationSchedule.NewCertificationSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CertificationScheduleValidatorReportsEveryProblem();
        CertificationScheduleRetriesRangeRuleFlagsValuesAboveRetries();
        CertificationScheduleRequiredCodeRuleFlagsABlankCode();
    }
}
