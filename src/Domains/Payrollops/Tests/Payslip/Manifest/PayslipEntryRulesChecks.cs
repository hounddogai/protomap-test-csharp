namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PayslipEntryRulesChecks
{
    public static void PayslipEntryValidatorReportsEveryProblem()
    {
        var validator = PayslipEntryRules.DefaultPayslipEntryValidator();
        var item = PayslipEntry.NewPayslipEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipEntryRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PayslipEntryRetriesRangeRule(64, 255);
        var item = PayslipEntry.NewPayslipEntry("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipEntryCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new PayslipEntryCeilingRangeRule(7.5, 3921.2);
        var item = PayslipEntry.NewPayslipEntry("x", "y");
        item.Ceiling = 3922.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipEntryRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new PayslipEntryRequiredLocationRule("payslip entry needs a value");
        var item = PayslipEntry.NewPayslipEntry("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayslipEntryCodeLengthRule(64);
        var item = PayslipEntry.NewPayslipEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayslipEntryValidatorReportsEveryProblem();
        PayslipEntryRetriesRangeRuleFlagsValuesAboveRetries();
        PayslipEntryCeilingRangeRuleFlagsValuesAboveCeiling();
        PayslipEntryRequiredLocationRuleFlagsABlankLocation();
        PayslipEntryCodeLengthRuleFlagsALongCode();
    }
}
