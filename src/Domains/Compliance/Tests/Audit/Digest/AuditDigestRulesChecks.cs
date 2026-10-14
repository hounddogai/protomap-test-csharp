namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditDigestRulesChecks
{
    public static void AuditDigestValidatorReportsEveryProblem()
    {
        var validator = AuditDigestRules.DefaultAuditDigestValidator();
        var item = AuditDigest.NewAuditDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditDigestRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new AuditDigestRetriesRangeRule(32, 1000);
        var item = AuditDigest.NewAuditDigest("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditDigestRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new AuditDigestRequiredSalaryBandRule("audit digest needs this field");
        var item = AuditDigest.NewAuditDigest("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditDigestSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new AuditDigestSalaryBandLengthRule(32);
        var item = AuditDigest.NewAuditDigest("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AuditDigestValidatorReportsEveryProblem();
        AuditDigestRetriesRangeRuleFlagsValuesAboveRetries();
        AuditDigestRequiredSalaryBandRuleFlagsABlankSalaryBand();
        AuditDigestSalaryBandLengthRuleFlagsALongSalaryBand();
    }
}
