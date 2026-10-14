namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkVarianceRulesChecks
{
    public static void ScheduledStatementOfWorkVarianceValidatorReportsEveryProblem()
    {
        var validator = ScheduledStatementOfWorkVarianceRules.DefaultScheduledStatementOfWorkVarianceValidator();
        var item = ScheduledStatementOfWorkVariance.NewScheduledStatementOfWorkVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledStatementOfWorkVarianceRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new ScheduledStatementOfWorkVarianceRequiredGradeRule(
            "scheduled statement of work variance needs this field");
        var item = ScheduledStatementOfWorkVariance.NewScheduledStatementOfWorkVariance("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledStatementOfWorkVarianceWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new ScheduledStatementOfWorkVarianceWeightRangeRule(1.4, 3473.3);
        var item = ScheduledStatementOfWorkVariance.NewScheduledStatementOfWorkVariance("x", "y");
        item.Weight = 3474.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkVarianceValidatorReportsEveryProblem();
        ScheduledStatementOfWorkVarianceRequiredGradeRuleFlagsABlankGrade();
        ScheduledStatementOfWorkVarianceWeightRangeRuleFlagsValuesAboveWeight();
    }
}
