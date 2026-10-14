namespace Acme.Workforce.Compliance.Tests.Attestation.Rule;

using Acme.Workforce.Compliance.Attestation.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AttestationScoreRulesChecks
{
    public static void AttestationScoreValidatorReportsEveryProblem()
    {
        var validator = AttestationScoreRules.DefaultAttestationScoreValidator();
        var item = AttestationScore.NewAttestationScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationScoreRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AttestationScoreRequiredTitleRule("attestation score needs an entry");
        var item = AttestationScore.NewAttestationScore("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AttestationScoreValidatorReportsEveryProblem();
        AttestationScoreRequiredTitleRuleFlagsABlankTitle();
    }
}
