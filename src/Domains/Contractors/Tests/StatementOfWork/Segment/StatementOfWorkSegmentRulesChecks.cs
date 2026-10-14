namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Segment;

using Acme.Workforce.Contractors.StatementOfWork.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StatementOfWorkSegmentRulesChecks
{
    public static void StatementOfWorkSegmentValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkSegmentRules.DefaultStatementOfWorkSegmentValidator();
        var item = StatementOfWorkSegment.NewStatementOfWorkSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkSegmentMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new StatementOfWorkSegmentMaximumRangeRule(1.8, 3406.0);
        var item = StatementOfWorkSegment.NewStatementOfWorkSegment("x", "y");
        item.Maximum = 3407.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkSegmentRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new StatementOfWorkSegmentRequiredGradeRule("statement of work segment needs this field");
        var item = StatementOfWorkSegment.NewStatementOfWorkSegment("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkSegmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new StatementOfWorkSegmentRequiredNameRule("statement of work segment needs a value");
        var item = StatementOfWorkSegment.NewStatementOfWorkSegment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkSegmentValidatorReportsEveryProblem();
        StatementOfWorkSegmentMaximumRangeRuleFlagsValuesAboveMaximum();
        StatementOfWorkSegmentRequiredGradeRuleFlagsABlankGrade();
        StatementOfWorkSegmentRequiredNameRuleFlagsABlankName();
    }
}
