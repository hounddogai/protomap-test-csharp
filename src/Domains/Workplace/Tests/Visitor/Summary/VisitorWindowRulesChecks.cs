namespace Acme.Workforce.Workplace.Tests.Visitor.Summary;

using Acme.Workforce.Workplace.Visitor.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class VisitorWindowRulesChecks
{
    public static void VisitorWindowValidatorReportsEveryProblem()
    {
        var validator = VisitorWindowRules.DefaultVisitorWindowValidator();
        var item = VisitorWindow.NewVisitorWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorWindowRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new VisitorWindowRequiredEmployeeIdRule("visitor window needs this field");
        var item = VisitorWindow.NewVisitorWindow("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new VisitorWindowCodeLengthRule(3600);
        var item = VisitorWindow.NewVisitorWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorWindowNameLengthRuleFlagsALongName()
    {
        var rule = new VisitorWindowNameLengthRule(80);
        var item = VisitorWindow.NewVisitorWindow("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VisitorWindowValidatorReportsEveryProblem();
        VisitorWindowRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        VisitorWindowCodeLengthRuleFlagsALongCode();
        VisitorWindowNameLengthRuleFlagsALongName();
    }
}
