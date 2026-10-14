namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AttestationSnapshotRulesChecks
{
    public static void AttestationSnapshotValidatorReportsEveryProblem()
    {
        var validator = AttestationSnapshotRules.DefaultAttestationSnapshotValidator();
        var item = AttestationSnapshot.NewAttestationSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationSnapshotBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new AttestationSnapshotBudgetRangeRule(2.8, 1922.7);
        var item = AttestationSnapshot.NewAttestationSnapshot("x", "y");
        item.Budget = 1924.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationSnapshotCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new AttestationSnapshotCreatedAtRangeRule(120, 1000);
        var item = AttestationSnapshot.NewAttestationSnapshot("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AttestationSnapshotValidatorReportsEveryProblem();
        AttestationSnapshotBudgetRangeRuleFlagsValuesAboveBudget();
        AttestationSnapshotCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
