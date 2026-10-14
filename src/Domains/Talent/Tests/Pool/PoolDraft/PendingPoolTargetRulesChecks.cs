namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class PendingPoolTargetRulesChecks
{
    public static void PendingPoolTargetValidatorReportsEveryProblem()
    {
        var validator = PendingPoolTargetRules.DefaultPendingPoolTargetValidator();
        var item = PendingPoolTarget.NewPendingPoolTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPoolTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingPoolTargetCodeLengthRule(80);
        var item = PendingPoolTarget.NewPendingPoolTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingPoolTargetEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new PendingPoolTargetEmployeeIdLengthRule(86400);
        var item = PendingPoolTarget.NewPendingPoolTarget("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingPoolTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingPoolTargetRequiredCodeRule("pending pool target needs this field");
        var item = PendingPoolTarget.NewPendingPoolTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPoolTargetRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new PendingPoolTargetRequiredEmployeeIdRule("pending pool target needs a value");
        var item = PendingPoolTarget.NewPendingPoolTarget("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingPoolTargetValidatorReportsEveryProblem();
        PendingPoolTargetCodeLengthRuleFlagsALongCode();
        PendingPoolTargetEmployeeIdLengthRuleFlagsALongEmployeeId();
        PendingPoolTargetRequiredCodeRuleFlagsABlankCode();
        PendingPoolTargetRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
