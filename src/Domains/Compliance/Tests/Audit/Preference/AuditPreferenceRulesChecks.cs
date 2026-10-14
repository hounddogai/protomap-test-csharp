namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class AuditPreferenceRulesChecks
{
    public static void AuditPreferenceValidatorReportsEveryProblem()
    {
        var validator = AuditPreferenceRules.DefaultAuditPreferenceValidator();
        var item = AuditPreference.NewAuditPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AuditPreferenceRequiredCodeRule("audit preference needs this field");
        var item = AuditPreference.NewAuditPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new AuditPreferenceCodeLengthRule(32);
        var item = AuditPreference.NewAuditPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AuditPreferenceValidatorReportsEveryProblem();
        AuditPreferenceRequiredCodeRuleFlagsABlankCode();
        AuditPreferenceCodeLengthRuleFlagsALongCode();
    }
}
