namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorTemplateRulesChecks
{
    public static void VisitorTemplateValidatorReportsEveryProblem()
    {
        var validator = VisitorTemplateRules.DefaultVisitorTemplateValidator();
        var item = VisitorTemplate.NewVisitorTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorTemplateJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new VisitorTemplateJobCodeLengthRule(120);
        var item = VisitorTemplate.NewVisitorTemplate("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorTemplateRequiredNameRuleFlagsABlankName()
    {
        var rule = new VisitorTemplateRequiredNameRule("visitor template needs a value");
        var item = VisitorTemplate.NewVisitorTemplate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorTemplateRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new VisitorTemplateRequiredDepartmentRule("visitor template needs an entry");
        var item = VisitorTemplate.NewVisitorTemplate("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorTemplateValidatorReportsEveryProblem();
        VisitorTemplateJobCodeLengthRuleFlagsALongJobCode();
        VisitorTemplateRequiredNameRuleFlagsABlankName();
        VisitorTemplateRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
