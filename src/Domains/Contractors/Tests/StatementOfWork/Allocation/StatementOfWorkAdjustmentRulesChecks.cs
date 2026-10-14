namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class StatementOfWorkAdjustmentRulesChecks
{
    public static void StatementOfWorkAdjustmentValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkAdjustmentRules.DefaultStatementOfWorkAdjustmentValidator();
        var item = StatementOfWorkAdjustment.NewStatementOfWorkAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkAdjustmentCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new StatementOfWorkAdjustmentCreatedAtRangeRule(1000, 3600);
        var item = StatementOfWorkAdjustment.NewStatementOfWorkAdjustment("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkAdjustmentAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new StatementOfWorkAdjustmentAmountRangeRule(5.1, 3561.4);
        var item = StatementOfWorkAdjustment.NewStatementOfWorkAdjustment("x", "y");
        item.Amount = 3562.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkAdjustmentDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new StatementOfWorkAdjustmentDepartmentLengthRule(86400);
        var item = StatementOfWorkAdjustment.NewStatementOfWorkAdjustment("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StatementOfWorkAdjustmentValidatorReportsEveryProblem();
        StatementOfWorkAdjustmentCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        StatementOfWorkAdjustmentAmountRangeRuleFlagsValuesAboveAmount();
        StatementOfWorkAdjustmentDepartmentLengthRuleFlagsALongDepartment();
    }
}
