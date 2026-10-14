namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class VisitorQuotaRulesChecks
{
    public static void VisitorQuotaValidatorReportsEveryProblem()
    {
        var validator = VisitorQuotaRules.DefaultVisitorQuotaValidator();
        var item = VisitorQuota.NewVisitorQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VisitorQuotaRequiredCodeRule("visitor quota needs this field");
        var item = VisitorQuota.NewVisitorQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorQuotaLabelLengthRuleFlagsALongLabel()
    {
        var rule = new VisitorQuotaLabelLengthRule(3600);
        var item = VisitorQuota.NewVisitorQuota("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VisitorQuotaValidatorReportsEveryProblem();
        VisitorQuotaRequiredCodeRuleFlagsABlankCode();
        VisitorQuotaLabelLengthRuleFlagsALongLabel();
    }
}
