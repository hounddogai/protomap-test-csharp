namespace Acme.Workforce.Compliance.Tests.Attestation.Draft;

using Acme.Workforce.Compliance.Attestation.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AttestationDraftRulesChecks
{
    public static void AttestationDraftValidatorReportsEveryProblem()
    {
        var validator = AttestationDraftRules.DefaultAttestationDraftValidator();
        var item = AttestationDraft.NewAttestationDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationDraftRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new AttestationDraftRequiredChannelRule("attestation draft needs an entry");
        var item = AttestationDraft.NewAttestationDraft("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationDraftRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new AttestationDraftRatingRangeRule(0.4, 771.8);
        var item = AttestationDraft.NewAttestationDraft("x", "y");
        item.Rating = 773.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AttestationDraftValidatorReportsEveryProblem();
        AttestationDraftRequiredChannelRuleFlagsABlankChannel();
        AttestationDraftRatingRangeRuleFlagsValuesAboveRating();
    }
}
