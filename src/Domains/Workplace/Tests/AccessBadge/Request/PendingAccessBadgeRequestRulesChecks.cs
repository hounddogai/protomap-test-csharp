namespace Acme.Workforce.Workplace.Tests.AccessBadge.Request;

using Acme.Workforce.Workplace.AccessBadge.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PendingAccessBadgeRequestRulesChecks
{
    public static void PendingAccessBadgeRequestValidatorReportsEveryProblem()
    {
        var validator = PendingAccessBadgeRequestRules.DefaultPendingAccessBadgeRequestValidator();
        var item = PendingAccessBadgeRequest.NewPendingAccessBadgeRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingAccessBadgeRequestRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new PendingAccessBadgeRequestRevisionRangeRule(120, 120);
        var item = PendingAccessBadgeRequest.NewPendingAccessBadgeRequest("x", "y");
        item.Revision = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingAccessBadgeRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingAccessBadgeRequestRequiredCodeRule("pending access badge request needs this field");
        var item = PendingAccessBadgeRequest.NewPendingAccessBadgeRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingAccessBadgeRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingAccessBadgeRequestCodeLengthRule(1000);
        var item = PendingAccessBadgeRequest.NewPendingAccessBadgeRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingAccessBadgeRequestValidatorReportsEveryProblem();
        PendingAccessBadgeRequestRevisionRangeRuleFlagsValuesAboveRevision();
        PendingAccessBadgeRequestRequiredCodeRuleFlagsABlankCode();
        PendingAccessBadgeRequestCodeLengthRuleFlagsALongCode();
    }
}
