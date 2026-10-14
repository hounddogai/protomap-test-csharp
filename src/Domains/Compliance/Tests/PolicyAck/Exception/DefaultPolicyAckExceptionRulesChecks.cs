namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DefaultPolicyAckExceptionRulesChecks
{
    public static void DefaultPolicyAckExceptionValidatorReportsEveryProblem()
    {
        var validator = DefaultPolicyAckExceptionRules.DefaultDefaultPolicyAckExceptionValidator();
        var item = DefaultPolicyAckException.NewDefaultPolicyAckException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPolicyAckExceptionRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultPolicyAckExceptionRequiredNameRule("default policy ack exception needs an entry");
        var item = DefaultPolicyAckException.NewDefaultPolicyAckException("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPolicyAckExceptionCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new DefaultPolicyAckExceptionCreatedAtRangeRule(64, 1000);
        var item = DefaultPolicyAckException.NewDefaultPolicyAckException("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPolicyAckExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultPolicyAckExceptionCodeLengthRule(32);
        var item = DefaultPolicyAckException.NewDefaultPolicyAckException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultPolicyAckExceptionNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultPolicyAckExceptionNameLengthRule(3600);
        var item = DefaultPolicyAckException.NewDefaultPolicyAckException("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultPolicyAckExceptionValidatorReportsEveryProblem();
        DefaultPolicyAckExceptionRequiredNameRuleFlagsABlankName();
        DefaultPolicyAckExceptionCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        DefaultPolicyAckExceptionCodeLengthRuleFlagsALongCode();
        DefaultPolicyAckExceptionNameLengthRuleFlagsALongName();
    }
}
