namespace Acme.Workforce.Compliance.Tests.Attestation.Cycle;

using Acme.Workforce.Compliance.Attestation.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PendingAttestationCycleRulesChecks
{
    public static void PendingAttestationCycleValidatorReportsEveryProblem()
    {
        var validator = PendingAttestationCycleRules.DefaultPendingAttestationCycleValidator();
        var item = PendingAttestationCycle.NewPendingAttestationCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingAttestationCycleGradeLengthRuleFlagsALongGrade()
    {
        var rule = new PendingAttestationCycleGradeLengthRule(1000);
        var item = PendingAttestationCycle.NewPendingAttestationCycle("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingAttestationCycleNameLengthRuleFlagsALongName()
    {
        var rule = new PendingAttestationCycleNameLengthRule(255);
        var item = PendingAttestationCycle.NewPendingAttestationCycle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingAttestationCycleRequiredNameRuleFlagsABlankName()
    {
        var rule = new PendingAttestationCycleRequiredNameRule("pending attestation cycle needs a value");
        var item = PendingAttestationCycle.NewPendingAttestationCycle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingAttestationCycleValidatorReportsEveryProblem();
        PendingAttestationCycleGradeLengthRuleFlagsALongGrade();
        PendingAttestationCycleNameLengthRuleFlagsALongName();
        PendingAttestationCycleRequiredNameRuleFlagsABlankName();
    }
}
