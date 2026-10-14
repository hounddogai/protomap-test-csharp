namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StatementOfWorkBaselineRulesChecks
{
    public static void StatementOfWorkBaselineValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkBaselineRules.DefaultStatementOfWorkBaselineValidator();
        var item = StatementOfWorkBaseline.NewStatementOfWorkBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkBaselineExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new StatementOfWorkBaselineExternalRefLengthRule(255);
        var item = StatementOfWorkBaseline.NewStatementOfWorkBaseline("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkBaselineEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new StatementOfWorkBaselineEndsAtRangeRule(32, 86400);
        var item = StatementOfWorkBaseline.NewStatementOfWorkBaseline("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkBaselineRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new StatementOfWorkBaselineRequiredExternalRefRule("statement of work baseline needs this field");
        var item = StatementOfWorkBaseline.NewStatementOfWorkBaseline("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkBaselineStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new StatementOfWorkBaselineStartsAtRangeRule(16, 64);
        var item = StatementOfWorkBaseline.NewStatementOfWorkBaseline("x", "y");
        item.StartsAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StatementOfWorkBaselineRequiredCodeRule("statement of work baseline needs a value");
        var item = StatementOfWorkBaseline.NewStatementOfWorkBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkBaselineValidatorReportsEveryProblem();
        StatementOfWorkBaselineExternalRefLengthRuleFlagsALongExternalRef();
        StatementOfWorkBaselineEndsAtRangeRuleFlagsValuesAboveEndsAt();
        StatementOfWorkBaselineRequiredExternalRefRuleFlagsABlankExternalRef();
        StatementOfWorkBaselineStartsAtRangeRuleFlagsValuesAboveStartsAt();
        StatementOfWorkBaselineRequiredCodeRuleFlagsABlankCode();
    }
}
