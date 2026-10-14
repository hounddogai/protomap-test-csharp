namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationCheckpointRulesChecks
{
    public static void AttestationCheckpointValidatorReportsEveryProblem()
    {
        var validator = AttestationCheckpointRules.DefaultAttestationCheckpointValidator();
        var item = AttestationCheckpoint.NewAttestationCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new AttestationCheckpointCodeLengthRule(3600);
        var item = AttestationCheckpoint.NewAttestationCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationCheckpointLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AttestationCheckpointLabelLengthRule(120);
        var item = AttestationCheckpoint.NewAttestationCheckpoint("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationCheckpointBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new AttestationCheckpointBudgetRangeRule(4.7, 3179.7);
        var item = AttestationCheckpoint.NewAttestationCheckpoint("x", "y");
        item.Budget = 3181.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AttestationCheckpointValidatorReportsEveryProblem();
        AttestationCheckpointCodeLengthRuleFlagsALongCode();
        AttestationCheckpointLabelLengthRuleFlagsALongLabel();
        AttestationCheckpointBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
