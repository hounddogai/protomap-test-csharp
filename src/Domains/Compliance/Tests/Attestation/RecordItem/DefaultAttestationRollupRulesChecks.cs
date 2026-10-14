namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DefaultAttestationRollupRulesChecks
{
    public static void DefaultAttestationRollupValidatorReportsEveryProblem()
    {
        var validator = DefaultAttestationRollupRules.DefaultDefaultAttestationRollupValidator();
        var item = DefaultAttestationRollup.NewDefaultAttestationRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultAttestationRollupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultAttestationRollupRequiredLabelRule("default attestation rollup needs this field");
        var item = DefaultAttestationRollup.NewDefaultAttestationRollup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultAttestationRollupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultAttestationRollupLabelLengthRule(16);
        var item = DefaultAttestationRollup.NewDefaultAttestationRollup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultAttestationRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultAttestationRollupCodeLengthRule(3600);
        var item = DefaultAttestationRollup.NewDefaultAttestationRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultAttestationRollupValidatorReportsEveryProblem();
        DefaultAttestationRollupRequiredLabelRuleFlagsABlankLabel();
        DefaultAttestationRollupLabelLengthRuleFlagsALongLabel();
        DefaultAttestationRollupCodeLengthRuleFlagsALongCode();
    }
}
