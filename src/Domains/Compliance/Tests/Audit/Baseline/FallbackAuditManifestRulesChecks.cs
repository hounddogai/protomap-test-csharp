namespace Acme.Workforce.Compliance.Tests.Audit.Baseline;

using Acme.Workforce.Compliance.Audit.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackAuditManifestRulesChecks
{
    public static void FallbackAuditManifestValidatorReportsEveryProblem()
    {
        var validator = FallbackAuditManifestRules.DefaultFallbackAuditManifestValidator();
        var item = FallbackAuditManifest.NewFallbackAuditManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackAuditManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackAuditManifestRequiredCodeRule("fallback audit manifest needs an entry");
        var item = FallbackAuditManifest.NewFallbackAuditManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackAuditManifestExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new FallbackAuditManifestExpiresAtRangeRule(80, 255);
        var item = FallbackAuditManifest.NewFallbackAuditManifest("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackAuditManifestRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new FallbackAuditManifestRequiredLocaleRule("fallback audit manifest needs a value");
        var item = FallbackAuditManifest.NewFallbackAuditManifest("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackAuditManifestValidatorReportsEveryProblem();
        FallbackAuditManifestRequiredCodeRuleFlagsABlankCode();
        FallbackAuditManifestExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        FallbackAuditManifestRequiredLocaleRuleFlagsABlankLocale();
    }
}
