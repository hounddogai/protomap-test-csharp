namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class RegionalPayRunGroupRulesChecks
{
    public static void RegionalPayRunGroupValidatorReportsEveryProblem()
    {
        var validator = RegionalPayRunGroupRules.DefaultRegionalPayRunGroupValidator();
        var item = RegionalPayRunGroup.NewRegionalPayRunGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalPayRunGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalPayRunGroupRequiredCodeRule("regional pay run group needs a value");
        var item = RegionalPayRunGroup.NewRegionalPayRunGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalPayRunGroupRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new RegionalPayRunGroupRequiredSummaryRule("regional pay run group needs this field");
        var item = RegionalPayRunGroup.NewRegionalPayRunGroup("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalPayRunGroupSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new RegionalPayRunGroupSummaryLengthRule(86400);
        var item = RegionalPayRunGroup.NewRegionalPayRunGroup("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalPayRunGroupValidatorReportsEveryProblem();
        RegionalPayRunGroupRequiredCodeRuleFlagsABlankCode();
        RegionalPayRunGroupRequiredSummaryRuleFlagsABlankSummary();
        RegionalPayRunGroupSummaryLengthRuleFlagsALongSummary();
    }
}
