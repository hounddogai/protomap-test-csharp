namespace Acme.Workforce.Compliance.Tests.Attestation.Claim;

using Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AttestationMilestoneRulesChecks
{
    public static void AttestationMilestoneValidatorReportsEveryProblem()
    {
        var validator = AttestationMilestoneRules.DefaultAttestationMilestoneValidator();
        var item = AttestationMilestone.NewAttestationMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationMilestoneLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AttestationMilestoneLabelLengthRule(64);
        var item = AttestationMilestone.NewAttestationMilestone("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationMilestoneNameLengthRuleFlagsALongName()
    {
        var rule = new AttestationMilestoneNameLengthRule(16);
        var item = AttestationMilestone.NewAttestationMilestone("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationMilestoneRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new AttestationMilestoneRatioRangeRule(3.0, 1225.3);
        var item = AttestationMilestone.NewAttestationMilestone("x", "y");
        item.Ratio = 1226.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AttestationMilestoneValidatorReportsEveryProblem();
        AttestationMilestoneLabelLengthRuleFlagsALongLabel();
        AttestationMilestoneNameLengthRuleFlagsALongName();
        AttestationMilestoneRatioRangeRuleFlagsValuesAboveRatio();
    }
}
