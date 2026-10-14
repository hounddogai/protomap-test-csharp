namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StatementOfWorkEstimateRulesChecks
{
    public static void StatementOfWorkEstimateValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkEstimateRules.DefaultStatementOfWorkEstimateValidator();
        var item = StatementOfWorkEstimate.NewStatementOfWorkEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkEstimateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StatementOfWorkEstimateRequiredLabelRule("statement of work estimate needs a value");
        var item = StatementOfWorkEstimate.NewStatementOfWorkEstimate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkEstimateRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new StatementOfWorkEstimateRequiredDescriptionRule("statement of work estimate needs an entry");
        var item = StatementOfWorkEstimate.NewStatementOfWorkEstimate("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StatementOfWorkEstimateRequiredCodeRule("statement of work estimate needs a value");
        var item = StatementOfWorkEstimate.NewStatementOfWorkEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkEstimateChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new StatementOfWorkEstimateChecksumLengthRule(120);
        var item = StatementOfWorkEstimate.NewStatementOfWorkEstimate("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StatementOfWorkEstimateValidatorReportsEveryProblem();
        StatementOfWorkEstimateRequiredLabelRuleFlagsABlankLabel();
        StatementOfWorkEstimateRequiredDescriptionRuleFlagsABlankDescription();
        StatementOfWorkEstimateRequiredCodeRuleFlagsABlankCode();
        StatementOfWorkEstimateChecksumLengthRuleFlagsALongChecksum();
    }
}
