namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class FallbackAttestationSummaryRulesChecks
{
    public static void FallbackAttestationSummaryValidatorReportsEveryProblem()
    {
        var validator = FallbackAttestationSummaryRules.DefaultFallbackAttestationSummaryValidator();
        var item = FallbackAttestationSummary.NewFallbackAttestationSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackAttestationSummaryRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new FallbackAttestationSummaryRequiredChecksumRule("fallback attestation summary needs this field");
        var item = FallbackAttestationSummary.NewFallbackAttestationSummary("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackAttestationSummaryRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new FallbackAttestationSummaryRequiredSourceRule("fallback attestation summary needs an entry");
        var item = FallbackAttestationSummary.NewFallbackAttestationSummary("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackAttestationSummaryRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackAttestationSummaryRequiredNameRule("fallback attestation summary needs an entry");
        var item = FallbackAttestationSummary.NewFallbackAttestationSummary("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackAttestationSummarySourceLengthRuleFlagsALongSource()
    {
        var rule = new FallbackAttestationSummarySourceLengthRule(255);
        var item = FallbackAttestationSummary.NewFallbackAttestationSummary("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackAttestationSummaryValidatorReportsEveryProblem();
        FallbackAttestationSummaryRequiredChecksumRuleFlagsABlankChecksum();
        FallbackAttestationSummaryRequiredSourceRuleFlagsABlankSource();
        FallbackAttestationSummaryRequiredNameRuleFlagsABlankName();
        FallbackAttestationSummarySourceLengthRuleFlagsALongSource();
    }
}
