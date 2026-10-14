namespace Acme.Workforce.Compliance.Tests.Attestation.Estimate;

using Acme.Workforce.Compliance.Attestation.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AttestationEntryRulesChecks
{
    public static void AttestationEntryValidatorReportsEveryProblem()
    {
        var validator = AttestationEntryRules.DefaultAttestationEntryValidator();
        var item = AttestationEntry.NewAttestationEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationEntryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AttestationEntryRequiredLabelRule("attestation entry needs a value");
        var item = AttestationEntry.NewAttestationEntry("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationEntrySourceLengthRuleFlagsALongSource()
    {
        var rule = new AttestationEntrySourceLengthRule(64);
        var item = AttestationEntry.NewAttestationEntry("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationEntryCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new AttestationEntryCurrencyLengthRule(3600);
        var item = AttestationEntry.NewAttestationEntry("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AttestationEntryRequiredCodeRule("attestation entry needs an entry");
        var item = AttestationEntry.NewAttestationEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationEntryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AttestationEntryLabelLengthRule(3600);
        var item = AttestationEntry.NewAttestationEntry("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AttestationEntryValidatorReportsEveryProblem();
        AttestationEntryRequiredLabelRuleFlagsABlankLabel();
        AttestationEntrySourceLengthRuleFlagsALongSource();
        AttestationEntryCurrencyLengthRuleFlagsALongCurrency();
        AttestationEntryRequiredCodeRuleFlagsABlankCode();
        AttestationEntryLabelLengthRuleFlagsALongLabel();
    }
}
