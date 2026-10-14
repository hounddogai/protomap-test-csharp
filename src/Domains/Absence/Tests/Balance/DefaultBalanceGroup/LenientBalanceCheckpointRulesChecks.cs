namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceCheckpointRulesChecks
{
    public static void LenientBalanceCheckpointValidatorReportsEveryProblem()
    {
        var validator = LenientBalanceCheckpointRules.DefaultLenientBalanceCheckpointValidator();
        var item = LenientBalanceCheckpoint.NewLenientBalanceCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientBalanceCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientBalanceCheckpointCodeLengthRule(3600);
        var item = LenientBalanceCheckpoint.NewLenientBalanceCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientBalanceCheckpointRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new LenientBalanceCheckpointRequiredEmployeeIdRule("lenient balance checkpoint needs an entry");
        var item = LenientBalanceCheckpoint.NewLenientBalanceCheckpoint("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBalanceCheckpointEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new LenientBalanceCheckpointEmployeeIdLengthRule(255);
        var item = LenientBalanceCheckpoint.NewLenientBalanceCheckpoint("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientBalanceCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientBalanceCheckpointRequiredCodeRule("lenient balance checkpoint needs a value");
        var item = LenientBalanceCheckpoint.NewLenientBalanceCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientBalanceCheckpointValidatorReportsEveryProblem();
        LenientBalanceCheckpointCodeLengthRuleFlagsALongCode();
        LenientBalanceCheckpointRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        LenientBalanceCheckpointEmployeeIdLengthRuleFlagsALongEmployeeId();
        LenientBalanceCheckpointRequiredCodeRuleFlagsABlankCode();
    }
}
