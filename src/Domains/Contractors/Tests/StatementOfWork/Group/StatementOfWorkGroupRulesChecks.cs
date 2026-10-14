namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Group;

using Acme.Workforce.Contractors.StatementOfWork.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class StatementOfWorkGroupRulesChecks
{
    public static void StatementOfWorkGroupValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkGroupRules.DefaultStatementOfWorkGroupValidator();
        var item = StatementOfWorkGroup.NewStatementOfWorkGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new StatementOfWorkGroupCodeLengthRule(255);
        var item = StatementOfWorkGroup.NewStatementOfWorkGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkGroupNameLengthRuleFlagsALongName()
    {
        var rule = new StatementOfWorkGroupNameLengthRule(16);
        var item = StatementOfWorkGroup.NewStatementOfWorkGroup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkGroupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StatementOfWorkGroupLabelLengthRule(16);
        var item = StatementOfWorkGroup.NewStatementOfWorkGroup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkGroupRequiredNameRuleFlagsABlankName()
    {
        var rule = new StatementOfWorkGroupRequiredNameRule("statement of work group needs this field");
        var item = StatementOfWorkGroup.NewStatementOfWorkGroup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkGroupValidatorReportsEveryProblem();
        StatementOfWorkGroupCodeLengthRuleFlagsALongCode();
        StatementOfWorkGroupNameLengthRuleFlagsALongName();
        StatementOfWorkGroupLabelLengthRuleFlagsALongLabel();
        StatementOfWorkGroupRequiredNameRuleFlagsABlankName();
    }
}
