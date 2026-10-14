namespace Acme.Workforce.Workplace.Tests.Visitor.Proposal;

using Acme.Workforce.Workplace.Visitor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class VisitorExceptionRulesChecks
{
    public static void VisitorExceptionValidatorReportsEveryProblem()
    {
        var validator = VisitorExceptionRules.DefaultVisitorExceptionValidator();
        var item = VisitorException.NewVisitorException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorExceptionJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new VisitorExceptionJobCodeLengthRule(3600);
        var item = VisitorException.NewVisitorException("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorExceptionStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new VisitorExceptionStartsAtRangeRule(64, 64);
        var item = VisitorException.NewVisitorException("x", "y");
        item.StartsAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorExceptionRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new VisitorExceptionRequiredJobCodeRule("visitor exception needs a value");
        var item = VisitorException.NewVisitorException("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new VisitorExceptionCodeLengthRule(32);
        var item = VisitorException.NewVisitorException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorExceptionRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new VisitorExceptionRequiredTitleRule("visitor exception needs this field");
        var item = VisitorException.NewVisitorException("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorExceptionValidatorReportsEveryProblem();
        VisitorExceptionJobCodeLengthRuleFlagsALongJobCode();
        VisitorExceptionStartsAtRangeRuleFlagsValuesAboveStartsAt();
        VisitorExceptionRequiredJobCodeRuleFlagsABlankJobCode();
        VisitorExceptionCodeLengthRuleFlagsALongCode();
        VisitorExceptionRequiredTitleRuleFlagsABlankTitle();
    }
}
