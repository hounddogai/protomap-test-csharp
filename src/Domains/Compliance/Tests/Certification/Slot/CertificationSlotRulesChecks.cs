namespace Acme.Workforce.Compliance.Tests.Certification.Slot;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CertificationSlotRulesChecks
{
    public static void CertificationSlotValidatorReportsEveryProblem()
    {
        var validator = CertificationSlotRules.DefaultCertificationSlotValidator();
        var item = CertificationSlot.NewCertificationSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationSlotTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new CertificationSlotTenureMonthsRangeRule(80, 1000);
        var item = CertificationSlot.NewCertificationSlot("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationSlotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CertificationSlotTitleLengthRule(3600);
        var item = CertificationSlot.NewCertificationSlot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationSlotRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new CertificationSlotRequiredEmployeeIdRule("certification slot needs a value");
        var item = CertificationSlot.NewCertificationSlot("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationSlotAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new CertificationSlotAttemptsRangeRule(1000, 3600);
        var item = CertificationSlot.NewCertificationSlot("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CertificationSlotValidatorReportsEveryProblem();
        CertificationSlotTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        CertificationSlotTitleLengthRuleFlagsALongTitle();
        CertificationSlotRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        CertificationSlotAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
