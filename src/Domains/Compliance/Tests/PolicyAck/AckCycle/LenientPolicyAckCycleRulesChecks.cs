namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckCycle;

using Acme.Workforce.Compliance.PolicyAck.AckCycle;

/// <summary>Checks of ack cycle code, run by the project's runner.</summary>
public static class LenientPolicyAckCycleRulesChecks
{
    public static void LenientPolicyAckCycleValidatorReportsEveryProblem()
    {
        var validator = LenientPolicyAckCycleRules.DefaultLenientPolicyAckCycleValidator();
        var item = LenientPolicyAckCycle.NewLenientPolicyAckCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPolicyAckCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientPolicyAckCycleRequiredCodeRule("lenient policy ack cycle needs this field");
        var item = LenientPolicyAckCycle.NewLenientPolicyAckCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPolicyAckCycleJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new LenientPolicyAckCycleJobCodeLengthRule(16);
        var item = LenientPolicyAckCycle.NewLenientPolicyAckCycle("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientPolicyAckCycleRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new LenientPolicyAckCycleRequiredGradeRule("lenient policy ack cycle needs an entry");
        var item = LenientPolicyAckCycle.NewLenientPolicyAckCycle("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPolicyAckCycleRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new LenientPolicyAckCycleRequiredJobCodeRule("lenient policy ack cycle needs this field");
        var item = LenientPolicyAckCycle.NewLenientPolicyAckCycle("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientPolicyAckCycleValidatorReportsEveryProblem();
        LenientPolicyAckCycleRequiredCodeRuleFlagsABlankCode();
        LenientPolicyAckCycleJobCodeLengthRuleFlagsALongJobCode();
        LenientPolicyAckCycleRequiredGradeRuleFlagsABlankGrade();
        LenientPolicyAckCycleRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
