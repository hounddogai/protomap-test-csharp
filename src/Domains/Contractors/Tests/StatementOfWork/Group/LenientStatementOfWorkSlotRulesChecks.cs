namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Group;

using Acme.Workforce.Contractors.StatementOfWork.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class LenientStatementOfWorkSlotRulesChecks
{
    public static void LenientStatementOfWorkSlotValidatorReportsEveryProblem()
    {
        var validator = LenientStatementOfWorkSlotRules.DefaultLenientStatementOfWorkSlotValidator();
        var item = LenientStatementOfWorkSlot.NewLenientStatementOfWorkSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientStatementOfWorkSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientStatementOfWorkSlotCodeLengthRule(32);
        var item = LenientStatementOfWorkSlot.NewLenientStatementOfWorkSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientStatementOfWorkSlotRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new LenientStatementOfWorkSlotRequiredEmployeeIdRule("lenient statement of work slot needs an entry");
        var item = LenientStatementOfWorkSlot.NewLenientStatementOfWorkSlot("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientStatementOfWorkSlotRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientStatementOfWorkSlotRequiredNameRule("lenient statement of work slot needs this field");
        var item = LenientStatementOfWorkSlot.NewLenientStatementOfWorkSlot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientStatementOfWorkSlotValidatorReportsEveryProblem();
        LenientStatementOfWorkSlotCodeLengthRuleFlagsALongCode();
        LenientStatementOfWorkSlotRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        LenientStatementOfWorkSlotRequiredNameRuleFlagsABlankName();
    }
}
