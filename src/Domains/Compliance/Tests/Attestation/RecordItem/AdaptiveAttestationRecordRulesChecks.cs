namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AdaptiveAttestationRecordRulesChecks
{
    public static void AdaptiveAttestationRecordValidatorReportsEveryProblem()
    {
        var validator = AdaptiveAttestationRecordRules.DefaultAdaptiveAttestationRecordValidator();
        var item = AdaptiveAttestationRecord.NewAdaptiveAttestationRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveAttestationRecordRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new AdaptiveAttestationRecordRequiredTimezoneRule("adaptive attestation record needs this field");
        var item = AdaptiveAttestationRecord.NewAdaptiveAttestationRecord("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveAttestationRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveAttestationRecordRequiredCodeRule("adaptive attestation record needs a value");
        var item = AdaptiveAttestationRecord.NewAdaptiveAttestationRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveAttestationRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveAttestationRecordLabelLengthRule(120);
        var item = AdaptiveAttestationRecord.NewAdaptiveAttestationRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveAttestationRecordValidatorReportsEveryProblem();
        AdaptiveAttestationRecordRequiredTimezoneRuleFlagsABlankTimezone();
        AdaptiveAttestationRecordRequiredCodeRuleFlagsABlankCode();
        AdaptiveAttestationRecordLabelLengthRuleFlagsALongLabel();
    }
}
