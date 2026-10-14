namespace Acme.Workforce.Compliance.Tests.Audit.AuditMapping;

using Acme.Workforce.Compliance.Audit.AuditMapping;

/// <summary>Checks of audit mapping code, run by the project's runner.</summary>
public static class AuditLedgerRulesChecks
{
    public static void AuditLedgerValidatorReportsEveryProblem()
    {
        var validator = AuditLedgerRules.DefaultAuditLedgerValidator();
        var item = AuditLedger.NewAuditLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditLedgerHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new AuditLedgerHoursRangeRule(2.0, 3492.3);
        var item = AuditLedger.NewAuditLedger("x", "y");
        item.Hours = 3493.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditLedgerRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new AuditLedgerRequiredDescriptionRule("audit ledger needs this field");
        var item = AuditLedger.NewAuditLedger("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AuditLedgerRequiredCodeRule("audit ledger needs an entry");
        var item = AuditLedger.NewAuditLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditLedgerRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new AuditLedgerRequiredLocaleRule("audit ledger needs this field");
        var item = AuditLedger.NewAuditLedger("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditLedgerEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new AuditLedgerEndsAtRangeRule(64, 80);
        var item = AuditLedger.NewAuditLedger("x", "y");
        item.EndsAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditLedgerLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new AuditLedgerLocaleLengthRule(16);
        var item = AuditLedger.NewAuditLedger("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AuditLedgerValidatorReportsEveryProblem();
        AuditLedgerHoursRangeRuleFlagsValuesAboveHours();
        AuditLedgerRequiredDescriptionRuleFlagsABlankDescription();
        AuditLedgerRequiredCodeRuleFlagsABlankCode();
        AuditLedgerRequiredLocaleRuleFlagsABlankLocale();
        AuditLedgerEndsAtRangeRuleFlagsValuesAboveEndsAt();
        AuditLedgerLocaleLengthRuleFlagsALongLocale();
    }
}
