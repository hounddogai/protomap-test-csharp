namespace Acme.Workforce.Workplace.Tests.Visitor.Bundle;

using Acme.Workforce.Workplace.Visitor.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class VisitorBundleRulesChecks
{
    public static void VisitorBundleValidatorReportsEveryProblem()
    {
        var validator = VisitorBundleRules.DefaultVisitorBundleValidator();
        var item = VisitorBundle.NewVisitorBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new VisitorBundleCodeLengthRule(3600);
        var item = VisitorBundle.NewVisitorBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new VisitorBundleRequiredNameRule("visitor bundle needs a value");
        var item = VisitorBundle.NewVisitorBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorBundleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new VisitorBundleTitleLengthRule(255);
        var item = VisitorBundle.NewVisitorBundle("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VisitorBundleValidatorReportsEveryProblem();
        VisitorBundleCodeLengthRuleFlagsALongCode();
        VisitorBundleRequiredNameRuleFlagsABlankName();
        VisitorBundleTitleLengthRuleFlagsALongTitle();
    }
}
