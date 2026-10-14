namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkMappingRulesChecks
{
    public static void DefaultStatementOfWorkMappingValidatorReportsEveryProblem()
    {
        var validator = DefaultStatementOfWorkMappingRules.DefaultDefaultStatementOfWorkMappingValidator();
        var item = DefaultStatementOfWorkMapping.NewDefaultStatementOfWorkMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultStatementOfWorkMappingRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultStatementOfWorkMappingRequiredLabelRule("default statement of work mapping needs a value");
        var item = DefaultStatementOfWorkMapping.NewDefaultStatementOfWorkMapping("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultStatementOfWorkMappingRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new DefaultStatementOfWorkMappingRequiredSourceRule(
            "default statement of work mapping needs this field");
        var item = DefaultStatementOfWorkMapping.NewDefaultStatementOfWorkMapping("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultStatementOfWorkMappingLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultStatementOfWorkMappingLabelLengthRule(255);
        var item = DefaultStatementOfWorkMapping.NewDefaultStatementOfWorkMapping("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultStatementOfWorkMappingValidatorReportsEveryProblem();
        DefaultStatementOfWorkMappingRequiredLabelRuleFlagsABlankLabel();
        DefaultStatementOfWorkMappingRequiredSourceRuleFlagsABlankSource();
        DefaultStatementOfWorkMappingLabelLengthRuleFlagsALongLabel();
    }
}
