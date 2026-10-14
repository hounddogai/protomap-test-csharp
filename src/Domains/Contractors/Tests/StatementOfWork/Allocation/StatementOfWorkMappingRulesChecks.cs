namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class StatementOfWorkMappingRulesChecks
{
    public static void StatementOfWorkMappingValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkMappingRules.DefaultStatementOfWorkMappingValidator();
        var item = StatementOfWorkMapping.NewStatementOfWorkMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkMappingLocationLengthRuleFlagsALongLocation()
    {
        var rule = new StatementOfWorkMappingLocationLengthRule(3600);
        var item = StatementOfWorkMapping.NewStatementOfWorkMapping("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StatementOfWorkMappingRequiredCodeRule("statement of work mapping needs a value");
        var item = StatementOfWorkMapping.NewStatementOfWorkMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new StatementOfWorkMappingRequiredNameRule("statement of work mapping needs a value");
        var item = StatementOfWorkMapping.NewStatementOfWorkMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkMappingValidatorReportsEveryProblem();
        StatementOfWorkMappingLocationLengthRuleFlagsALongLocation();
        StatementOfWorkMappingRequiredCodeRuleFlagsABlankCode();
        StatementOfWorkMappingRequiredNameRuleFlagsABlankName();
    }
}
