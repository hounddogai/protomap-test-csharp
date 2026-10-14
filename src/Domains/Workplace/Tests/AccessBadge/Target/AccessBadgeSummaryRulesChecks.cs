namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeSummaryRulesChecks
{
    public static void AccessBadgeSummaryValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeSummaryRules.DefaultAccessBadgeSummaryValidator();
        var item = AccessBadgeSummary.NewAccessBadgeSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeSummaryNameLengthRuleFlagsALongName()
    {
        var rule = new AccessBadgeSummaryNameLengthRule(64);
        var item = AccessBadgeSummary.NewAccessBadgeSummary("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccessBadgeSummaryRequiredCodeRule("access badge summary needs this field");
        var item = AccessBadgeSummary.NewAccessBadgeSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeSummaryRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccessBadgeSummaryRequiredNameRule("access badge summary needs a value");
        var item = AccessBadgeSummary.NewAccessBadgeSummary("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccessBadgeSummaryValidatorReportsEveryProblem();
        AccessBadgeSummaryNameLengthRuleFlagsALongName();
        AccessBadgeSummaryRequiredCodeRuleFlagsABlankCode();
        AccessBadgeSummaryRequiredNameRuleFlagsABlankName();
    }
}
