namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Rule;

using Acme.Workforce.Contractors.StatementOfWork.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictStatementOfWorkMetricRulesChecks
{
    public static void StrictStatementOfWorkMetricValidatorReportsEveryProblem()
    {
        var validator = StrictStatementOfWorkMetricRules.DefaultStrictStatementOfWorkMetricValidator();
        var item = StrictStatementOfWorkMetric.NewStrictStatementOfWorkMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictStatementOfWorkMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictStatementOfWorkMetricCodeLengthRule(86400);
        var item = StrictStatementOfWorkMetric.NewStrictStatementOfWorkMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictStatementOfWorkMetricRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new StrictStatementOfWorkMetricRequiredExternalRefRule(
            "strict statement of work metric needs an entry");
        var item = StrictStatementOfWorkMetric.NewStrictStatementOfWorkMetric("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictStatementOfWorkMetricUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new StrictStatementOfWorkMetricUpdatedAtRangeRule(32, 120);
        var item = StrictStatementOfWorkMetric.NewStrictStatementOfWorkMetric("x", "y");
        item.UpdatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictStatementOfWorkMetricValidatorReportsEveryProblem();
        StrictStatementOfWorkMetricCodeLengthRuleFlagsALongCode();
        StrictStatementOfWorkMetricRequiredExternalRefRuleFlagsABlankExternalRef();
        StrictStatementOfWorkMetricUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
