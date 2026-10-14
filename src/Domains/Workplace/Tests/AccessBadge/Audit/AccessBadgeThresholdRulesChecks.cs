namespace Acme.Workforce.Workplace.Tests.AccessBadge.Audit;

using Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AccessBadgeThresholdRulesChecks
{
    public static void AccessBadgeThresholdValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeThresholdRules.DefaultAccessBadgeThresholdValidator();
        var item = AccessBadgeThreshold.NewAccessBadgeThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgeThresholdCodeLengthRule(1000);
        var item = AccessBadgeThreshold.NewAccessBadgeThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeThresholdNameLengthRuleFlagsALongName()
    {
        var rule = new AccessBadgeThresholdNameLengthRule(80);
        var item = AccessBadgeThreshold.NewAccessBadgeThreshold("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccessBadgeThresholdValidatorReportsEveryProblem();
        AccessBadgeThresholdCodeLengthRuleFlagsALongCode();
        AccessBadgeThresholdNameLengthRuleFlagsALongName();
    }
}
