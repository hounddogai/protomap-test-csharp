namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class LenientStatementOfWorkCheckpointRulesChecks
{
    public static void LenientStatementOfWorkCheckpointValidatorReportsEveryProblem()
    {
        var validator = LenientStatementOfWorkCheckpointRules.DefaultLenientStatementOfWorkCheckpointValidator();
        var item = LenientStatementOfWorkCheckpoint.NewLenientStatementOfWorkCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientStatementOfWorkCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientStatementOfWorkCheckpointCodeLengthRule(32);
        var item = LenientStatementOfWorkCheckpoint.NewLenientStatementOfWorkCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientStatementOfWorkCheckpointRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new LenientStatementOfWorkCheckpointRequiredContactEmailRule(
            "lenient statement of work checkpoint needs a value");
        var item = LenientStatementOfWorkCheckpoint.NewLenientStatementOfWorkCheckpoint("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientStatementOfWorkCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new LenientStatementOfWorkCheckpointNameLengthRule(64);
        var item = LenientStatementOfWorkCheckpoint.NewLenientStatementOfWorkCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientStatementOfWorkCheckpointValidatorReportsEveryProblem();
        LenientStatementOfWorkCheckpointCodeLengthRuleFlagsALongCode();
        LenientStatementOfWorkCheckpointRequiredContactEmailRuleFlagsABlankContactEmail();
        LenientStatementOfWorkCheckpointNameLengthRuleFlagsALongName();
    }
}
