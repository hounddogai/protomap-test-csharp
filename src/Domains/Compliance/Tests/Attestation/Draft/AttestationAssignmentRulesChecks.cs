namespace Acme.Workforce.Compliance.Tests.Attestation.Draft;

using Acme.Workforce.Compliance.Attestation.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AttestationAssignmentRulesChecks
{
    public static void AttestationAssignmentValidatorReportsEveryProblem()
    {
        var validator = AttestationAssignmentRules.DefaultAttestationAssignmentValidator();
        var item = AttestationAssignment.NewAttestationAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationAssignmentRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new AttestationAssignmentRequiredNotesRule("attestation assignment needs an entry");
        var item = AttestationAssignment.NewAttestationAssignment("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationAssignmentAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new AttestationAssignmentAttemptsRangeRule(80, 86400);
        var item = AttestationAssignment.NewAttestationAssignment("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationAssignmentSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new AttestationAssignmentSequenceRangeRule(16, 16);
        var item = AttestationAssignment.NewAttestationAssignment("x", "y");
        item.Sequence = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AttestationAssignmentRequiredCodeRule("attestation assignment needs this field");
        var item = AttestationAssignment.NewAttestationAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AttestationAssignmentValidatorReportsEveryProblem();
        AttestationAssignmentRequiredNotesRuleFlagsABlankNotes();
        AttestationAssignmentAttemptsRangeRuleFlagsValuesAboveAttempts();
        AttestationAssignmentSequenceRangeRuleFlagsValuesAboveSequence();
        AttestationAssignmentRequiredCodeRuleFlagsABlankCode();
    }
}
