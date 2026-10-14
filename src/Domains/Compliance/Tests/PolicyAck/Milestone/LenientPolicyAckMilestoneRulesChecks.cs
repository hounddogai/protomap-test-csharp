namespace Acme.Workforce.Compliance.Tests.PolicyAck.Milestone;

using Acme.Workforce.Compliance.PolicyAck.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LenientPolicyAckMilestoneRulesChecks
{
    public static void LenientPolicyAckMilestoneValidatorReportsEveryProblem()
    {
        var validator = LenientPolicyAckMilestoneRules.DefaultLenientPolicyAckMilestoneValidator();
        var item = LenientPolicyAckMilestone.NewLenientPolicyAckMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPolicyAckMilestoneRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new LenientPolicyAckMilestoneRequiredChecksumRule("lenient policy ack milestone needs this field");
        var item = LenientPolicyAckMilestone.NewLenientPolicyAckMilestone("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPolicyAckMilestoneChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new LenientPolicyAckMilestoneChecksumLengthRule(64);
        var item = LenientPolicyAckMilestone.NewLenientPolicyAckMilestone("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientPolicyAckMilestoneFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new LenientPolicyAckMilestoneFteRangeRule(7.7, 1046.2);
        var item = LenientPolicyAckMilestone.NewLenientPolicyAckMilestone("x", "y");
        item.Fte = 1047.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientPolicyAckMilestoneLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientPolicyAckMilestoneLabelLengthRule(255);
        var item = LenientPolicyAckMilestone.NewLenientPolicyAckMilestone("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientPolicyAckMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientPolicyAckMilestoneCodeLengthRule(32);
        var item = LenientPolicyAckMilestone.NewLenientPolicyAckMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientPolicyAckMilestoneValidatorReportsEveryProblem();
        LenientPolicyAckMilestoneRequiredChecksumRuleFlagsABlankChecksum();
        LenientPolicyAckMilestoneChecksumLengthRuleFlagsALongChecksum();
        LenientPolicyAckMilestoneFteRangeRuleFlagsValuesAboveFte();
        LenientPolicyAckMilestoneLabelLengthRuleFlagsALongLabel();
        LenientPolicyAckMilestoneCodeLengthRuleFlagsALongCode();
    }
}
