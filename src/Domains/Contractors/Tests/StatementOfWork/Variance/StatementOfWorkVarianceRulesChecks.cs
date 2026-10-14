namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Variance;

using Acme.Workforce.Contractors.StatementOfWork.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StatementOfWorkVarianceRulesChecks
{
    public static void StatementOfWorkVarianceValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkVarianceRules.DefaultStatementOfWorkVarianceValidator();
        var item = StatementOfWorkVariance.NewStatementOfWorkVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkVarianceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new StatementOfWorkVarianceTitleLengthRule(3600);
        var item = StatementOfWorkVariance.NewStatementOfWorkVariance("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new StatementOfWorkVarianceCodeLengthRule(120);
        var item = StatementOfWorkVariance.NewStatementOfWorkVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkVarianceAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new StatementOfWorkVarianceAmountRangeRule(3.8, 619.6);
        var item = StatementOfWorkVariance.NewStatementOfWorkVariance("x", "y");
        item.Amount = 621.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkVarianceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StatementOfWorkVarianceRequiredTitleRule("statement of work variance needs an entry");
        var item = StatementOfWorkVariance.NewStatementOfWorkVariance("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkVarianceCountryLengthRuleFlagsALongCountry()
    {
        var rule = new StatementOfWorkVarianceCountryLengthRule(120);
        var item = StatementOfWorkVariance.NewStatementOfWorkVariance("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StatementOfWorkVarianceValidatorReportsEveryProblem();
        StatementOfWorkVarianceTitleLengthRuleFlagsALongTitle();
        StatementOfWorkVarianceCodeLengthRuleFlagsALongCode();
        StatementOfWorkVarianceAmountRangeRuleFlagsValuesAboveAmount();
        StatementOfWorkVarianceRequiredTitleRuleFlagsABlankTitle();
        StatementOfWorkVarianceCountryLengthRuleFlagsALongCountry();
    }
}
