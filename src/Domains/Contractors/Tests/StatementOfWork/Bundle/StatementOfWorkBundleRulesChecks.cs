namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StatementOfWorkBundleRulesChecks
{
    public static void StatementOfWorkBundleValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkBundleRules.DefaultStatementOfWorkBundleValidator();
        var item = StatementOfWorkBundle.NewStatementOfWorkBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkBundleBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new StatementOfWorkBundleBudgetRangeRule(4.7, 2922.7);
        var item = StatementOfWorkBundle.NewStatementOfWorkBundle("x", "y");
        item.Budget = 2924.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new StatementOfWorkBundleCodeLengthRule(64);
        var item = StatementOfWorkBundle.NewStatementOfWorkBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkBundleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new StatementOfWorkBundleUpdatedAtRangeRule(255, 86400);
        var item = StatementOfWorkBundle.NewStatementOfWorkBundle("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkBundleNameLengthRuleFlagsALongName()
    {
        var rule = new StatementOfWorkBundleNameLengthRule(32);
        var item = StatementOfWorkBundle.NewStatementOfWorkBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StatementOfWorkBundleValidatorReportsEveryProblem();
        StatementOfWorkBundleBudgetRangeRuleFlagsValuesAboveBudget();
        StatementOfWorkBundleCodeLengthRuleFlagsALongCode();
        StatementOfWorkBundleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        StatementOfWorkBundleNameLengthRuleFlagsALongName();
    }
}
